using System;
using System.Collections.Generic;

namespace Havoc
{
    public readonly struct HkSdkVersion
    {
        public static readonly HkSdkVersion V20150100 = new HkSdkVersion( 2015, 01, 00 );
        public static readonly HkSdkVersion V20160100 = new HkSdkVersion( 2016, 01, 00 );
        public static readonly HkSdkVersion V20160200 = new HkSdkVersion( 2016, 02, 00 );

        public static readonly IReadOnlyCollection<HkSdkVersion> SupportedSdkVersions = new[]
        {
            V20150100, V20160100, V20160200
        };

        public readonly uint Value;

        public ushort Year => ( ushort ) ( Value >> 16 );
        public ushort Major => ( byte ) ( ( Value >> 8 ) & 0xFF );
        public ushort Minor => ( byte ) ( Value & 0xFF );

        public bool Equals( HkSdkVersion other )
        {
            return other.Value == Value;
        }

        public override bool Equals( object obj )
        {
            return obj is HkSdkVersion other && Equals( other );
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Year:0000}{Major:00}{Minor:00}";
        }

        public static bool operator <( HkSdkVersion left, HkSdkVersion right )
        {
            return left.Value < right.Value;
        }

        public static bool operator >( HkSdkVersion left, HkSdkVersion right )
        {
            return left.Value > right.Value;
        }

        public static bool operator >=( HkSdkVersion left, HkSdkVersion right )
        {
            return left.Value >= right.Value;
        }

        public static bool operator <=( HkSdkVersion left, HkSdkVersion right )
        {
            return left.Value <= right.Value;
        }

        public static bool operator ==( HkSdkVersion left, HkSdkVersion right )
        {
            return left.Value == right.Value;
        }

        public static bool operator !=( HkSdkVersion left, HkSdkVersion right )
        {
            return left.Value != right.Value;
        }

        public HkSdkVersion( ushort year, byte major, byte minor )
        {
            Value = ( uint ) ( ( year << 16 ) | ( major << 8 ) | minor );
        }

        public HkSdkVersion( string value )
        {
            if ( value.Length != 8 )
                throw new ArgumentException( "Invalid SDK version string length.", nameof( value ) );

            var span = value.AsSpan();
            Value = ( uint ) ( ( ushort.Parse( span.Slice( 0, 4 ) ) << 16 ) |
                               ( byte.Parse( span.Slice( 4, 2 ) ) << 8 ) | byte.Parse( span.Slice( 6, 2 ) ) );
        }
    }
}