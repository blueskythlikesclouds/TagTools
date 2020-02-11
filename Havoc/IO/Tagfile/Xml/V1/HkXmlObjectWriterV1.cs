using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Havoc.Collections;
using Havoc.Extensions;
using Havoc.Objects;
using Havoc.Reflection;

namespace Havoc.IO.Tagfile.Xml.V1
{
    public class HkXmlObjectWriterV1 : IHkXmlObjectWriter
    {
        private readonly OrderedSet<IHkObject> mObjects;

        public HkXmlObjectWriterV1( IHkObject rootObject )
        {
            TypeWriter = new HkXmlTypeWriterV1( new HkTypeCompendium( rootObject ) );
            RootObject = rootObject;

            mObjects = new OrderedSet<IHkObject>();
            AddObjectsRecursively( rootObject );
        }

        public IHkXmlTypeWriter TypeWriter { get; }
        public IHkObject RootObject { get; }
        public IReadOnlyList<IHkObject> Objects => mObjects;

        public void WriteObject( XmlWriter writer, IHkObject obj, bool writeObjectDefinition = false )
        {
            WriteObject( writer, obj, null, writeObjectDefinition );
        }

        public void WriteAllObjects( XmlWriter writer )
        {
            foreach ( var obj in mObjects )
                WriteObject( writer, obj, true );
        }

        public string GetObjectIdString( IHkObject obj )
        {
            return $"#{( obj != null && mObjects.IndexMap.TryGetValue( obj, out int index ) ? index + 1 : 0 ):D4}";
        }

        private void WriteObject( XmlWriter writer, IHkObject obj, string name, bool writeObjectDefinition = false )
        {
            string formatName = HkXmlTypeWriterV1.GetFormatName( obj.Type );

            if ( writeObjectDefinition )
                writer.WriteStartElement( "object", ( "id", GetObjectIdString( obj ) ),
                    ( "type", TypeWriter.GetTypeIdString( obj.Type ) ) );

            else if ( !string.IsNullOrEmpty( name ) )
                writer.WriteStartElement( formatName, ( "name", name ) );

            else
                writer.WriteStartElement( formatName );

            switch ( obj.Type.Format )
            {
                case HkTypeFormat.Void:
                case HkTypeFormat.Opaque:
                    break;

                case HkTypeFormat.Bool:
                case HkTypeFormat.Int:
                case HkTypeFormat.FloatingPoint:
                    writer.WriteString( GetPrimitiveString( obj ) );
                    break;

                case HkTypeFormat.String:
                    writer.WriteString( obj.GetValue<HkString, string>() );
                    break;

                case HkTypeFormat.Ptr:
                    writer.WriteString( GetObjectIdString( obj.GetValue<HkPtr, IHkObject>() ) );
                    break;

                case HkTypeFormat.Class:
                {
                    var fields = obj.GetValue<HkClass, IReadOnlyDictionary<HkField, IHkObject>>();

                    if ( obj.Type.Name.StartsWith( "hkQsTransform" ) )
                    {
                        // this is gonna look ugly
                        var translation = fields[ obj.Type.Fields[ 0 ] ]
                            .GetValue<HkArray, IReadOnlyList<IHkObject>>();

                        var rotation = fields[ obj.Type.Fields[ 1 ] ]
                            .GetValue<HkArray, IReadOnlyList<IHkObject>>();

                        var scale = fields[ obj.Type.Fields[ 2 ] ]
                            .GetValue<HkArray, IReadOnlyList<IHkObject>>();

                        writer.WriteString( string.Join( " ",
                            translation.Concat( rotation ).Concat( scale ).Select( GetPrimitiveString ) ) );
                    }

                    else
                    {
                        foreach ( var (field, fieldObject) in fields )
                        {
                            if ( ( field.Flags & HkFieldFlags.IsNotSerializable ) != 0 )
                                continue;

                            WriteObject( writer, fieldObject, field.Name );
                        }
                    }

                    break;
                }

                case HkTypeFormat.Array:
                {
                    var array = obj.GetValue<HkArray, IReadOnlyList<IHkObject>>();

                    if ( !formatName.StartsWith( "vec" ) )
                        writer.WriteAttributeString( "size", $"{array.Count}" );

                    if ( obj.Type.SubType.Format == HkTypeFormat.Bool || obj.Type.SubType.Format == HkTypeFormat.Int ||
                         obj.Type.SubType.Format == HkTypeFormat.FloatingPoint )
                        writer.WriteString( string.Join( " ", array.Select( GetPrimitiveString ) ) );
                    else
                        foreach ( var childObject in array )
                            WriteObject( writer, childObject );

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            writer.WriteEndElement();
        }

        private void AddObjectsRecursively( IHkObject obj )
        {
            if ( obj == null || mObjects.Contains( obj ) || obj.Value == null )
                return;

            switch ( obj.Type.Format )
            {
                case HkTypeFormat.Ptr:
                    var ptrObject = obj.GetValue<HkPtr, IHkObject>();
                    AddObjectsRecursively( ptrObject );
                    mObjects.Add( ptrObject );
                    break;

                case HkTypeFormat.Class:
                {
                    if ( obj == RootObject )
                        mObjects.Add( obj );

                    foreach ( var (_, fieldObject) in
                        obj.GetValue<HkClass, IReadOnlyDictionary<HkField, IHkObject>>() )
                        AddObjectsRecursively( fieldObject );

                    break;
                }

                case HkTypeFormat.Array:
                {
                    foreach ( var childObject in obj.GetValue<HkArray, IReadOnlyList<IHkObject>>() )
                        AddObjectsRecursively( childObject );

                    break;
                }
            }
        }

        private static string GetPrimitiveString( IHkObject obj )
        {
            switch ( obj.Type.Format )
            {
                case HkTypeFormat.Bool:
                    return obj.GetValue<HkBool, bool>() ? "1" : "0";

                case HkTypeFormat.Int:
                {
                    if ( obj.Type.BitCount != 64 || obj.Type.IsSigned ) 
                        return $"{obj.Value}";

                    ulong value = obj.GetValue<HkUInt64, ulong>();
                    unsafe
                    {
                        return $"{*( long* ) &value}";
                    }
                }

                case HkTypeFormat.FloatingPoint:
                {
                    if ( obj.Type.IsSingle )
                    {
                        float value = obj.GetValue<HkSingle, float>();
                        unsafe
                        {
                            return $"x{*( uint* ) &value:X8}";
                        }
                    }

                    if ( obj.Type.IsDouble )
                    {
                        double value = obj.GetValue<HkDouble, double>();
                        unsafe
                        {
                            return $"x{*( ulong* ) &value:X16}";
                        }
                    }

                    throw new InvalidDataException(
                        $"Unexpected floating point format: 0x{obj.Type.FormatInfo:X}" );
                }

                default:
                    throw new ArgumentException( "Expected an object of bool, int or floating point type.",
                        nameof( obj ) );
            }
        }
    }
}