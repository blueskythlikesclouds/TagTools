using System;
using System.Collections.Generic;

namespace Havoc.Reflection.Unmanaged
{
    public class HkuTypeConverter
    {
        private readonly Dictionary<IntPtr, HkType> mTypeCache;

        public HkuTypeConverter()
        {
            mTypeCache = new Dictionary<IntPtr, HkType>();
        }

        public unsafe HkType Convert( void* type )
        {
            if ( type == null )
                return null;

            if ( mTypeCache.TryGetValue( ( IntPtr ) type, out var managedType ) )
                return managedType;

            mTypeCache.Add( ( IntPtr ) type, managedType = new HkType() );

            managedType.Name = HkuType.GetName( type );
            managedType.ParentType = Convert( HkuType.GetParentType( type ) );

            if ( HkuOptionalStruct.HasOptionalValue( type, ( int ) HkuOptionalValues.FormatInfoOfType ) )
            {
                managedType.Flags |= HkTypeFlags.HasFormatInfo;
                managedType.mFormatInfo = HkuType.GetFormatInfo( type );
            }

            if ( HkuOptionalStruct.HasOptionalValue( type, ( int ) HkuOptionalValues.SubTypeOfType ) )
            {
                managedType.Flags |= HkTypeFlags.HasSubType;
                managedType.mSubType = Convert( HkuType.GetSubType( type ) );
            }

            if ( HkuOptionalStruct.HasOptionalValue( type, ( int ) HkuOptionalValues.VersionOfType ) )
            {
                managedType.Flags |= HkTypeFlags.HasVersion;
                managedType.mVersion = HkuType.GetVersion( type );
            }

            if ( HkuOptionalStruct.HasOptionalValue( type, ( int ) HkuOptionalValues.InterfacesOfType ) )
            {
                managedType.Flags |= HkTypeFlags.HasInterfaces;

                var array = HkuType.GetInterfaces( type );
                if ( array != null )
                    for ( int i = 0; i < HkuInterface.GetArrayCount( array ); i++ )
                    {
                        var uInterface = HkuInterface.GetArrayItem( array, i );
                        managedType.mInterfaces.Add( new HkInterface
                        {
                            Flags = ( int ) uInterface->Flags,
                            Type = Convert( uInterface->Type )
                        } );
                    }
            }

            if ( HkuOptionalStruct.HasOptionalValue( type, ( int ) HkuOptionalValues.ParametersOfType ) )
            {
                var array = HkuType.GetParameters( type );
                if ( array != null )
                    for ( int i = 0; i < HkuParameter.GetArrayCount( array ); i++ )
                    {
                        var uParameter = HkuParameter.GetArrayItem( array, i );
                        managedType.mParameters.Add( new HkParameter
                        {
                            Name = uParameter->GetName(),
                            Value = uParameter->IsIntValue
                                ? ( object ) ( long ) uParameter->Value
                                : Convert( uParameter->Value )
                        } );
                    }
            }

            if ( HkuOptionalStruct.HasOptionalValue( type, ( int ) HkuOptionalValues.ByteSizeOfType ) )
            {
                managedType.Flags |= HkTypeFlags.HasByteSize;
                managedType.mByteSize = HkuType.GetByteSize( type );
                managedType.mAlignment = HkuType.GetAlignment( type );
            }

            if ( HkuOptionalStruct.HasOptionalValue( type, ( int ) HkuOptionalValues.UnknownFlagsOfType ) )
            {
                managedType.Flags |= HkTypeFlags.HasUnknownFlags;
                managedType.mUnknownFlags = HkuType.GetUnknownFlags( type );
            }

            if ( HkuOptionalStruct.HasOptionalValue( type, ( int ) HkuOptionalValues.FieldsOfType ) )
            {
                managedType.Flags |= HkTypeFlags.HasFields;

                var array = HkuType.GetFields( type );
                if ( array != null )
                    for ( int i = 0; i < HkuField.GetArrayCount( array ); i++ )
                    {
                        var uField = HkuField.GetArrayItem( array, i );
                        managedType.mFields.Add( new HkField
                        {
                            ByteOffset = HkuField.GetByteOffset( uField ),
                            Flags = ( HkFieldFlags ) HkuField.GetFlags( uField ),
                            Name = HkuField.GetName( uField ),
                            Type = Convert( HkuField.GetType( uField ) )
                        } );
                    }
            }

            return managedType;
        }
    }
}