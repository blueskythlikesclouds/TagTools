using System;
using System.Buffers.Binary;
using System.IO;
using Havoc.Extensions;

namespace Havoc.IO.Tagfile.Binary.Sections
{
    public class HkSectionWriter : IDisposable
    {
        private readonly bool mHasSubSections;
        private readonly long mPosition;
        private readonly Stream mStream;
        private readonly BinaryWriter mWriter;

        public HkSectionWriter( BinaryWriter writer, string signature, bool hasSubSections )
        {
            mStream = writer.BaseStream;
            mWriter = writer;
            mPosition = mStream.Position;
            mHasSubSections = hasSubSections;

            mWriter.Write( 0 );
            mWriter.Write( signature, 4 );
        }

        public void Dispose()
        {
            mWriter.WriteAlignmentPadding( 4 );

            long current = mStream.Position;
            int flagsAndSize = ( int ) ( current - mPosition ) | ( mHasSubSections ? 0 : 1 << 30 );

            mStream.Seek( mPosition, SeekOrigin.Begin );
            mWriter.Write( BinaryPrimitives.ReverseEndianness( flagsAndSize ) );
            mStream.Seek( current, SeekOrigin.Begin );
        }
    }
}