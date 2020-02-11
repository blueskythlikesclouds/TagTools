using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Havoc.Extensions;
using Havoc.IO.Tagfile.Binary.Sections;
using Havoc.IO.Tagfile.Binary.Types;
using Havoc.Objects;
using Havoc.Reflection;

namespace Havoc.IO.Tagfile.Binary
{
    public class HkBinaryTagfileReader : IDisposable
    {
        private readonly bool mLeaveOpen;
        private readonly BinaryReader mReader;
        private readonly Stream mStream;
        private long mDataOffset;
        private List<Item> mItems;
        private List<HkType> mTypes;

        private HkBinaryTagfileReader( Stream stream, bool leaveOpen )
        {
            mStream = stream;
            mReader = new BinaryReader( mStream, Encoding.UTF8, true );
            mLeaveOpen = leaveOpen;
        }

        public void Dispose()
        {
            if ( !mLeaveOpen )
                mStream.Dispose();

            mReader.Dispose();
        }

        private void ReadTagSection( HkSection section )
        {
            foreach ( var subSection in section.SubSections )
                switch ( subSection.Signature )
                {
                    case "SDKV":
                    {
                        mStream.Seek( subSection.Position, SeekOrigin.Begin );

                        string sdkVersion = mReader.ReadString( 8 );
                        if ( !HkSdkVersion.SupportedSdkVersions.Contains( new HkSdkVersion( sdkVersion ) ) )
                            throw new NotSupportedException( $"Unsupported SDK version: {sdkVersion}" );
                        break;
                    }

                    case "DATA":
                        mDataOffset = subSection.Position;
                        break;

                    case "TYPE":
                        ReadTypeSection( subSection );
                        break;

                    case "INDX":
                        ReadIndexSection( subSection );
                        break;

                    default:
                        throw new InvalidDataException( $"Unexpected signature: {subSection.Signature}" );
                }
        }

        private void ReadTypeCompendiumSection( HkSection section )
        {
            // Very barebones
            foreach ( var subSection in section.SubSections )
                switch ( subSection.Signature )
                {
                    case "TCID":
                        break;

                    case "TYPE":
                        ReadTypeSection( subSection );
                        break;

                    default:
                        throw new InvalidDataException( $"Unexpected signature: {subSection.Signature}" );
                }
        }

        private void ReadTypeSection( HkSection section )
        {
            mTypes = HkBinaryTypeReader.ReadTypeSection( mReader, section );
        }

        private void ReadIndexSection( HkSection section )
        {
            foreach ( var subSection in section.SubSections )
                switch ( subSection.Signature )
                {
                    case "ITEM":
                    {
                        mStream.Seek( subSection.Position, SeekOrigin.Begin );

                        mItems = new List<Item>( ( int ) ( subSection.Length / 24 ) );
                        while ( mStream.Position < subSection.Position + subSection.Length )
                            mItems.Add( new Item( this ) );

                        break;
                    }

                    case "PTCH":
                        break;

                    default:
                        throw new InvalidDataException( $"Unexpected signature: {subSection.Signature}" );
                }
        }

        private void ReadRootSection()
        {
            var section = new HkSection( mReader );
            switch ( section.Signature )
            {
                case "TAG0":
                    ReadTagSection( section );
                    break;

                case "TCM0":
                    ReadTypeCompendiumSection( section );
                    break;

                default:
                    throw new InvalidDataException( $"Unexpected signature: {section.Signature}" );
            }
        }

        public static IHkObject Read( Stream source, bool leaveOpen = false )
        {
            using ( var reader = new HkBinaryTagfileReader( source, leaveOpen ) )
            {
                // stuff
                reader.ReadRootSection();
                return reader.mItems[ 1 ].Objects[ 0 ];
            }
        }

        public static IHkObject Read( string filePath )
        {
            using ( var source = File.OpenRead( filePath ) )
            {
                return Read( source );
            }
        }

        private class Item
        {
            private readonly HkBinaryTagfileReader mTag;

            private List<IHkObject> mObjects;

            public Item( HkBinaryTagfileReader tag )
            {
                mTag = tag;

                int typeIndex = mTag.mReader.ReadInt32() & 0xFFFFFF;
                Type = typeIndex == 0 ? null : tag.mTypes[ typeIndex - 1 ];
                Position = mTag.mReader.ReadUInt32() + tag.mDataOffset;
                Count = mTag.mReader.ReadInt32();
            }

            private HkType Type { get; }
            private long Position { get; }
            private int Count { get; }

            public IReadOnlyList<IHkObject> Objects
            {
                get
                {
                    if ( mObjects == null )
                        ReadThisObject();

                    return mObjects;
                }
            }

            private void ReadThisObject()
            {
                if ( mObjects != null )
                    return;

                mObjects = new List<IHkObject>( Count );
                for ( int i = 0; i < Count; i++ )
                    mObjects.Add( ReadObject( Type, Position + i * Type.ByteSize ) );
            }

            private IHkObject ReadObject( HkType type, long offset )
            {
                mTag.mStream.Seek( offset, SeekOrigin.Begin );

                switch ( type.Format )
                {
                    case HkTypeFormat.Void:
                        return new HkVoid( type );

                    case HkTypeFormat.Opaque:
                        return new HkOpaque( type );

                    case HkTypeFormat.Bool:
                    {
                        bool value;

                        switch ( type.BitCount )
                        {
                            case 8:
                                value = mTag.mReader.ReadByte() != 0;
                                break;

                            case 16:
                                value = mTag.mReader.ReadInt16() != 0;
                                break;

                            case 32:
                                value = mTag.mReader.ReadInt32() != 0;
                                break;

                            case 64:
                                value = mTag.mReader.ReadInt64() != 0;
                                break;

                            default:
                                throw new InvalidDataException( $"Unexpected bit count: {type.BitCount}" );
                        }

                        return new HkBool( type, value );
                    }

                    case HkTypeFormat.String:
                    {
                        string value;
                        if ( type.IsFixedSize )
                        {
                            value = mTag.mReader.ReadString( type.FixedSize );
                        }
                        else
                        {
                            var item = ReadItemIndex();
                            if ( item != null )
                            {
                                var stringBuilder = new StringBuilder( item.Count - 1 );
                                for ( int i = 0; i < item.Count - 1; i++ )
                                    stringBuilder.Append( ( char ) ( byte ) item[ i ].Value );

                                value = stringBuilder.ToString();
                            }
                            else
                            {
                                value = null;
                            }
                        }

                        return new HkString( type, value );
                    }

                    case HkTypeFormat.Int:
                    {
                        switch ( type.BitCount )
                        {
                            case 8:
                                return type.IsSigned
                                    ? new HkSByte( type, mTag.mReader.ReadSByte() )
                                    : ( IHkObject ) new HkByte( type, mTag.mReader.ReadByte() );

                            case 16:
                                return type.IsSigned
                                    ? new HkInt16( type, mTag.mReader.ReadInt16() )
                                    : ( IHkObject ) new HkUInt16( type, mTag.mReader.ReadUInt16() );

                            case 32:
                                return type.IsSigned
                                    ? new HkInt32( type, mTag.mReader.ReadInt32() )
                                    : ( IHkObject ) new HkUInt32( type, mTag.mReader.ReadUInt32() );

                            case 64:
                                return type.IsSigned
                                    ? new HkInt64( type, mTag.mReader.ReadInt64() )
                                    : ( IHkObject ) new HkUInt64( type, mTag.mReader.ReadUInt64() );

                            default:
                                throw new InvalidDataException( $"Unexpected bit count: {type.BitCount}" );
                        }
                    }

                    case HkTypeFormat.FloatingPoint:
                        return type.IsSingle ? new HkSingle( type, mTag.mReader.ReadSingle() ) :
                            type.IsDouble ? ( IHkObject ) new HkDouble( type, mTag.mReader.ReadDouble() ) :
                            throw new InvalidDataException( "Unexpected floating point format" );

                    case HkTypeFormat.Ptr:
                        return new HkPtr( type, ReadItemIndex()?[ 0 ] );

                    case HkTypeFormat.Class:
                        return new HkClass( type,
                            type.AllFields.ToDictionary( x => x,
                                x => ReadObject( x.Type, offset + x.ByteOffset ) ) );

                    case HkTypeFormat.Array:
                    {
                        if ( !type.IsFixedSize )
                            return new HkArray( type, ReadItemIndex() );

                        var array = new IHkObject[ type.FixedSize ];
                        for ( int i = 0; i < array.Length; i++ )
                            array[ i ] = ReadObject( type.SubType, offset + i * type.SubType.ByteSize );

                        return new HkArray( type, array );
                    }

                    default:
                        throw new ArgumentOutOfRangeException( nameof( type.Format ) );
                }

                IReadOnlyList<IHkObject> ReadItemIndex()
                {
                    int index = mTag.mReader.ReadInt32();
                    return index == 0 ? null : mTag.mItems[ index ].Objects;
                }
            }
        }
    }
}