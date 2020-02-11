using System.Collections.Generic;
using System.IO;
using Havoc.Extensions;
using Havoc.IO.Tagfile.Binary.Sections;
using Havoc.Reflection;

namespace Havoc.IO.Tagfile.Binary.Types
{
    public static class HkBinaryTypeReader
    {
        public static List<HkType> ReadTypeSection( BinaryReader reader, HkSection section )
        {
            var types = new List<HkType>();
            var typeStrings = new List<string>();
            var fieldStrings = new List<string>();

            foreach ( var subSection in section.SubSections )
            {
                reader.BaseStream.Seek( subSection.Position, SeekOrigin.Begin );

                switch ( subSection.Signature )
                {
                    case "TPTR":
                        break;

                    case "TSTR":
                    {
                        while ( reader.BaseStream.Position < subSection.Position + subSection.Length )
                            typeStrings.Add( reader.ReadNullTerminatedString() );

                        break;
                    }

                    case "TNAM":
                    case "TNA1":
                    {
                        int typeCount = ( int ) reader.ReadPackedInt();

                        types = new List<HkType>( typeCount );
                        for ( int i = 0; i < typeCount; i++ )
                            types.Add( new HkType() );

                        foreach ( var type in types )
                        {
                            type.Name = typeStrings[ ( int ) reader.ReadPackedInt() ];

                            int templateCount = ( int ) reader.ReadPackedInt();

                            type.mParameters.Capacity = templateCount;
                            for ( int j = 0; j < templateCount; j++ )
                            {
                                var templateInfo = new HkParameter
                                {
                                    Name = typeStrings[ ( int ) reader.ReadPackedInt() ]
                                };

                                if ( templateInfo.Name[ 0 ] == 't' )
                                    templateInfo.Value = ReadTypeIndex();
                                else
                                    templateInfo.Value = reader.ReadPackedInt();

                                type.mParameters.Add( templateInfo );
                            }
                        }

                        break;
                    }

                    case "FSTR":
                    {
                        while ( reader.BaseStream.Position < subSection.Position + subSection.Length )
                            fieldStrings.Add( reader.ReadNullTerminatedString() );

                        break;
                    }

                    case "TBOD":
                    case "TBDY":
                    {
                        while ( reader.BaseStream.Position < subSection.Position + subSection.Length )
                        {
                            var type = ReadTypeIndex();
                            if ( type == null )
                                continue;

                            type.ParentType = ReadTypeIndex();
                            type.Flags = ( HkTypeFlags ) reader.ReadPackedInt();

                            if ( ( type.Flags & HkTypeFlags.HasFormatInfo ) != 0 )
                                type.mFormatInfo = ( int ) reader.ReadPackedInt();

                            if ( ( type.Flags & HkTypeFlags.HasSubType ) != 0 )
                                type.mSubType = ReadTypeIndex();

                            if ( ( type.Flags & HkTypeFlags.HasVersion ) != 0 )
                                type.mVersion = ( int ) reader.ReadPackedInt();

                            if ( ( type.Flags & HkTypeFlags.HasByteSize ) != 0 )
                            {
                                type.mByteSize = ( int ) reader.ReadPackedInt();
                                type.mAlignment = ( int ) reader.ReadPackedInt();
                            }

                            if ( ( type.Flags & HkTypeFlags.HasUnknownFlags ) != 0 )
                                type.mUnknownFlags = ( int ) reader.ReadPackedInt();

                            if ( ( type.Flags & HkTypeFlags.HasFields ) != 0 )
                            {
                                int fieldCount = ( int ) reader.ReadPackedInt();

                                type.mFields.Capacity = fieldCount;
                                for ( int i = 0; i < fieldCount; i++ )
                                    type.mFields.Add( new HkField
                                    {
                                        Name = fieldStrings[ ( int ) reader.ReadPackedInt() ],
                                        Flags = ( HkFieldFlags ) reader.ReadPackedInt(),
                                        ByteOffset = ( int ) reader.ReadPackedInt(),
                                        Type = ReadTypeIndex()
                                    } );
                            }

                            if ( ( type.Flags & HkTypeFlags.HasInterfaces ) != 0 )
                            {
                                int interfaceCount = ( int ) reader.ReadPackedInt();

                                type.mInterfaces.Capacity = interfaceCount;
                                for ( int i = 0; i < interfaceCount; i++ )
                                    type.mInterfaces.Add( new HkInterface
                                    {
                                        Type = ReadTypeIndex(),
                                        Flags = ( int ) reader.ReadPackedInt()
                                    } );
                            }
                        }

                        break;
                    }

                    case "THSH":
                    {
                        int hashCount = ( int ) reader.ReadPackedInt();
                        for ( int i = 0; i < hashCount; i++ )
                            ReadTypeIndex().Hash = reader.ReadInt32();

                        break;
                    }

                    case "TPAD":
                        break;

                    default:
                        throw new InvalidDataException( $"Unexpected signature: {subSection.Signature}" );
                }
            }

            return types;

            HkType ReadTypeIndex()
            {
                long index = reader.ReadPackedInt();
                if ( index == 0 )
                    return null;

                return types[ ( int ) index - 1 ];
            }
        }
    }
}