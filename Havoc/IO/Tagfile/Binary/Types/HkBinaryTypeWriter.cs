using System.IO;
using System.Linq;
using Havoc.Collections;
using Havoc.Extensions;
using Havoc.IO.Tagfile.Binary.Sections;
using Havoc.Reflection;

namespace Havoc.IO.Tagfile.Binary.Types
{
    public static class HkBinaryTypeWriter
    {
        public static void WriteTypeSection( BinaryWriter writer, HkTypeCompendium typeCompendium,
            HkSdkVersion sdkVersion )
        {
            var typeStrings = new OrderedSet<string>();
            var fieldStrings = new OrderedSet<string>();

            foreach ( var type in typeCompendium )
            {
                typeStrings.Add( type.Name );
                foreach ( var parameter in type.mParameters )
                    typeStrings.Add( parameter.Name );

                foreach ( var field in type.mFields )
                    fieldStrings.Add( field.Name );
            }

            using ( var typeSection = new HkSectionWriter( writer, "TYPE", true ) )
            {
                using ( var typePointerSection = new HkSectionWriter( writer, "TPTR", false ) )
                {
                    for ( int i = 0; i < typeCompendium.Count; i++ )
                        writer.Write( 0ul );
                }

                using ( var typeStringSection = new HkSectionWriter( writer, "TSTR", false ) )
                {
                    foreach ( string typeString in typeStrings )
                        writer.WriteNullTerminatedString( typeString );
                }

                using ( var typeNameSection = new HkSectionWriter( writer,
                    sdkVersion >= HkSdkVersion.V20160200 ? "TNA1" : "TNAM", false ) )
                {
                    writer.WritePackedInt( typeCompendium.Count );

                    foreach ( var type in typeCompendium )
                    {
                        writer.WritePackedInt( typeStrings.IndexMap[ type.Name ] );
                        writer.WritePackedInt( type.mParameters.Count );

                        foreach ( var parameter in type.mParameters )
                        {
                            writer.WritePackedInt( typeStrings.IndexMap[ parameter.Name ] );
                            if ( parameter.IsInt )
                                writer.WritePackedInt( ( long ) parameter.Value );
                            else
                                writer.WritePackedInt( GetTypeIndex( ( HkType ) parameter.Value ) );
                        }
                    }
                }

                using ( var fieldStringSection = new HkSectionWriter( writer, "FSTR", false ) )
                {
                    foreach ( string fieldString in fieldStrings )
                        writer.WriteNullTerminatedString( fieldString );
                }

                using ( var typeBodySection = new HkSectionWriter( writer,
                    sdkVersion >= HkSdkVersion.V20160200 ? "TBDY" : "TBOD", false ) )
                {
                    foreach ( var type in typeCompendium )
                    {
                        writer.WritePackedInt( GetTypeIndex( type ) );
                        writer.WritePackedInt( GetTypeIndex( type.ParentType ) );
                        writer.WritePackedInt( ( long ) type.Flags );

                        if ( ( type.Flags & HkTypeFlags.HasFormatInfo ) != 0 )
                            writer.WritePackedInt( type.mFormatInfo );

                        if ( ( type.Flags & HkTypeFlags.HasSubType ) != 0 )
                            writer.WritePackedInt( GetTypeIndex( type.mSubType ) );

                        if ( ( type.Flags & HkTypeFlags.HasVersion ) != 0 )
                            writer.WritePackedInt( type.mVersion );

                        if ( ( type.Flags & HkTypeFlags.HasByteSize ) != 0 )
                        {
                            writer.WritePackedInt( type.mByteSize );
                            writer.WritePackedInt( type.mAlignment );
                        }

                        if ( ( type.Flags & HkTypeFlags.HasUnknownFlags ) != 0 )
                            writer.WritePackedInt( type.mUnknownFlags );

                        if ( ( type.Flags & HkTypeFlags.HasFields ) != 0 )
                        {
                            writer.WritePackedInt( type.mFields.Count );

                            foreach ( var field in type.mFields )
                            {
                                writer.WritePackedInt( fieldStrings.IndexMap[ field.Name ] );
                                writer.WritePackedInt( ( long ) field.Flags );
                                writer.WritePackedInt( field.ByteOffset );
                                writer.WritePackedInt( GetTypeIndex( field.Type ) );
                            }
                        }

                        if ( ( type.Flags & HkTypeFlags.HasInterfaces ) != 0 )
                        {
                            writer.WritePackedInt( type.mInterfaces.Count );

                            foreach ( var item in type.mInterfaces )
                            {
                                writer.WritePackedInt( GetTypeIndex( item.Type ) );
                                writer.WritePackedInt( item.Flags );
                            }
                        }
                    }
                }

                using ( var typeHashSection = new HkSectionWriter( writer, "THSH", false ) )
                {
                    writer.WritePackedInt( typeCompendium.Count( x => x.Hash != 0 ) );
                    foreach ( var type in typeCompendium )
                    {
                        if ( type.Hash == 0 )
                            continue;

                        writer.WritePackedInt( GetTypeIndex( type ) );
                        writer.Write( type.Hash );
                    }
                }

                using var typePaddingSection = new HkSectionWriter( writer, "TPAD", false );
            }

            int GetTypeIndex( HkType type )
            {
                return type != null && typeCompendium.IndexMap.TryGetValue( type, out int index ) ? index + 1 : 0;
            }
        }
    }
}