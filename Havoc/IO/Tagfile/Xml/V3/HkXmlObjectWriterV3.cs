using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using Havoc.Collections;
using Havoc.Extensions;
using Havoc.Objects;
using Havoc.Reflection;

namespace Havoc.IO.Tagfile.Xml.V3
{
    public class HkXmlObjectWriterV3 : IHkXmlObjectWriter
    {
        private readonly OrderedSet<IHkObject> mObjects;

        public HkXmlObjectWriterV3( IHkObject rootObject )
        {
            TypeWriter = new HkXmlTypeWriterV3( new HkTypeCompendium( rootObject ) );
            RootObject = rootObject;

            mObjects = new OrderedSet<IHkObject>();
            AddObjectsRecursively( rootObject );
        }

        public IHkXmlTypeWriter TypeWriter { get; }
        public IHkObject RootObject { get; }

        public IReadOnlyList<IHkObject> Objects => mObjects;

        public void WriteObject( XmlWriter writer, IHkObject obj, bool writeObjectDefinition = false )
        {
            if ( writeObjectDefinition )
                writer.WriteStartElement( "object", obj.Type, ( "id", GetObjectIdString( obj ) ),
                    ( "typeid", TypeWriter.GetTypeIdString( obj.Type ) ) );

            switch ( obj.Type.Format )
            {
                case HkTypeFormat.Void:
                case HkTypeFormat.Opaque:
                    break;

                case HkTypeFormat.Bool:
                    writer.WriteElement( "bool", ( "value", obj.GetValue<HkBool, bool>() ? "true" : "false" ) );
                    break;

                case HkTypeFormat.String:
                {
                    string value = obj.Value != null ? obj.GetValue<HkString, string>() : null;
                    if ( !string.IsNullOrEmpty( value ) )
                        writer.WriteElement( "string", ( "value", value ) );
                    else
                        writer.WriteElement( "string" );

                    break;
                }

                case HkTypeFormat.Int:
                    writer.WriteElement( "integer", ( "value", obj.Value ) );
                    break;

                case HkTypeFormat.FloatingPoint:
                {
                    if ( obj.Type.IsSingle )
                    {
                        float value = obj.GetValue<HkSingle, float>();
                        unsafe
                        {
                            writer.WriteElement( "real", ( "dec", value.ToString( CultureInfo.InvariantCulture ) ),
                                ( "hex", $"#{*( uint* ) &value:X}" ) );
                        }
                    }
                    else if ( obj.Type.IsDouble )
                    {
                        double value = obj.GetValue<HkDouble, double>();
                        unsafe
                        {
                            writer.WriteElement( "real", ( "dec", value.ToString( CultureInfo.InvariantCulture ) ),
                                ( "hex", $"#{*( ulong* ) &value:X}" ) );
                        }
                    }
                    else
                    {
                        throw new InvalidDataException(
                            $"Unexpected floating point format: 0x{obj.Type.FormatInfo:X}" );
                    }

                    break;
                }

                case HkTypeFormat.Ptr:
                    writer.WriteElement( "pointer",
                        ( "id", GetObjectIdString( obj.Value != null ? obj.GetValue<HkPtr, IHkObject>() : null ) ) );
                    break;

                case HkTypeFormat.Class:
                {
                    writer.WriteStartElement( "record", obj.Type );
                    {
                        foreach ( var (field, fieldObject) in obj
                            .GetValue<HkClass, IReadOnlyDictionary<HkField, IHkObject>>() )
                        {
                            if ( ( field.Flags & HkFieldFlags.IsNotSerializable ) != 0 ||
                                 !fieldObject.IsWorthWriting() )
                                continue;

                            writer.WriteStartElement( "field", ( "name", field.Name ) );
                            {
                                WriteObject( writer, fieldObject );
                            }
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                    break;
                }

                case HkTypeFormat.Array:
                {
                    var array = obj.Value != null ? obj.GetValue<HkArray, IReadOnlyList<IHkObject>>() : null;

                    writer.WriteStartElement( "array", $"ArrayOf {obj.Type.SubType}", ( "count", array?.Count ?? 0 ),
                        ( "elementtypeid", TypeWriter.GetTypeIdString( obj.Type.SubType ) ) );

                    if ( array != null )
                        foreach ( var childObject in array )
                            WriteObject( writer, childObject );

                    writer.WriteEndElement();
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException( nameof( obj.Type.Format ) );
            }

            if ( writeObjectDefinition )
                writer.WriteEndElement();
        }

        public void WriteAllObjects( XmlWriter writer )
        {
            foreach ( var obj in mObjects )
                WriteObject( writer, obj, true );
        }

        public string GetObjectIdString( IHkObject obj )
        {
            return $"type{( obj != null && mObjects.IndexMap.TryGetValue( obj, out int index ) ? index + 1 : 0 )}";
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
    }
}