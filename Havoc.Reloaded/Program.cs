using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Havoc.IO.Tagfile.Binary.Types;
using Havoc.IO.Tagfile.Xml;
using Havoc.IO.Tagfile.Xml.V3;
using Havoc.Reflection;
using Havoc.Reflection.Unmanaged;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sigscan;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace Havoc.Reloaded
{
    public unsafe class Program : IMod
    {
        private TypeHasher* mA1;

        private Thread mCountWatcherThread;
        private IReloadedHooks mHooks;
        private ILogger mLogger;
        private IModLoader mModLoader;

        private void** mPreviousNode;

        private HkuTypeConverter mTypeConverter;
        private TypeHashCalculator mTypeHashCalculator;
        private TypeHasherCtor mTypeHasherCtor;

        private IHook<TypeNodeInitializer> mTypeNodeInitializerHook;
        private List<HkType> mTypes;

        private int mX = -1;
        private int mY = -1;

        public void Start( IModLoaderV1 loader )
        {
            mModLoader = ( IModLoader ) loader;
            mModLoader.GetController<IReloadedHooks>()?.TryGetTarget( out mHooks );
            mLogger = ( ILogger ) mModLoader.GetLogger();

            Debugger.Launch();

            var process = Process.GetCurrentProcess();
            var scanner = new Scanner( process, process.MainModule );

            var typeNodeInitializerScanResult = scanner.CompiledFindPattern( Patterns.TypeNodeInitializer );

            if ( !typeNodeInitializerScanResult.Found )
            {
                mLogger.PrintMessage( "[Havoc.Reloaded] Failed to locate type node initializer.", Color.Red );
                return;
            }

            long typeNodeInitializerAddress =
                ( long ) process.MainModule.BaseAddress + typeNodeInitializerScanResult.Offset;
            mLogger.PrintMessage(
                $"[Havoc.Reloaded] Found type node initializer at address 0x{typeNodeInitializerAddress:X}",
                Color.Green );

            mTypeConverter = new HkuTypeConverter();
            mTypes = new List<HkType>();

            // TODO: Make this work
            /*
            var typeHashCalculatorScanResult = scanner.CompiledFindPattern( Patterns.TypeHashCalculator );
            var typeHasherCtorScanResult = scanner.CompiledFindPattern( Patterns.TypeHasherCtor );

            if ( typeNodeInitializerScanResult.Found && typeHasherCtorScanResult.Found )
            {
                long typeHashCalculatorAddress =
                    ( long ) process.MainModule.BaseAddress + typeHashCalculatorScanResult.Offset;

                long typeHasherCtorAddress =
                    ( long ) process.MainModule.BaseAddress + typeHasherCtorScanResult.Offset;

                mLogger.PrintMessage(
                    $"[Havoc.Reloaded] Found type hash calculator at address 0x{typeHashCalculatorAddress:X}",
                    Color.Green );
                mLogger.PrintMessage(
                    $"[Havoc.Reloaded] Found type hasher constructor at address 0x{typeHasherCtorAddress:X}",
                    Color.Green );

                mTypeHashCalculator = mHooks.CreateWrapper<TypeHashCalculator>( typeHashCalculatorAddress, out _ );
                mTypeHasherCtor =
                    mHooks.CreateWrapper<TypeHasherCtor>( typeHasherCtorAddress, out _ );

                mA1 = ( TypeHasher* ) Marshal.AllocHGlobal( sizeof( TypeHasher ) );
                mTypeHasherCtor( mA1, -1 );
            }
            else
            {
                if ( !typeHashCalculatorScanResult.Found )
                    mLogger.PrintMessage( "[Havoc.Reloaded] Failed to locate type hash calculator.", Color.Red );

                if ( !typeHasherCtorScanResult.Found )
                    mLogger.PrintMessage( "[Havoc.Reloaded] Failed to locate type hasher constructor.", Color.Red );

                mLogger.PrintMessage( "[Havoc.Reloaded] The type compendium will not contain type hashes.",
                    Color.Yellow );
            }
            */

            mPreviousNode =
                ( void** ) ( typeNodeInitializerAddress + *( int* ) ( typeNodeInitializerAddress + 3 ) + 7 );

            mTypeNodeInitializerHook = mHooks
                .CreateHook<TypeNodeInitializer>( TypeNodeInitializerImpl, typeNodeInitializerAddress ).Activate();

            mCountWatcherThread = new Thread( CountWatcher );
            mCountWatcherThread.Start();

            void CountWatcher()
            {
                int lastCount = mTypes.Count;

                while ( true )
                {
                    Thread.Sleep( 250 );

                    if ( mTypes.Count == 0 )
                        continue;

                    if ( mTypes.Count != lastCount )
                    {
                        lastCount = mTypes.Count;
                        continue;
                    }

                    mLogger.PrintMessage( "[Havoc.Reloaded] Saving type compendium...", Color.Yellow );

                    string filePath = Path.ChangeExtension( process.MainModule.FileName, "compendium" );

                    using ( var compendium = File.Create( filePath ) )
                    using ( var writer = new BinaryWriter( compendium, Encoding.UTF8 ) )
                    {
                        HkBinaryTypeWriter.WriteTypeSection( writer, new HkTypeCompendium( mTypes ),
                            HkSdkVersion.V20150100 );
                    }

                    mLogger.PrintMessage(
                        $"[Havoc.Reloaded] Type compendium was successfully saved to location \"{filePath}\"",
                        Color.Green );

                    var xmlTypeWriter = new HkXmlTypeWriterV3( new HkTypeCompendium( mTypes ) );
                    xmlTypeWriter.WriteTypeCompendium( Path.ChangeExtension( process.MainModule.FileName,
                        ".compendium.xml" ) );

                    break;
                }
            }

            void* TypeNodeInitializerImpl( void* node, void* type )
            {
                mTypes.Add( mTypeConverter.Convert( type ) );

                if ( mX < 0 || mY < 0 )
                {
                    mX = Console.CursorLeft;
                    mY = Console.CursorTop;
                }
                else
                {
                    Console.SetCursorPosition( mX, mY );
                }

                mLogger.WriteLine( $"[Havoc.Reloaded] Converting Havok types... ({mTypes.Count})",
                    Color.Yellow );

                // Implementing the function itself here because calling it crashes for whatever reason.
                var data = ( void** ) node;

                data[ 0 ] = ( void* ) ( ( ulong ) *mPreviousNode & 0xFFFFFFFFFFFFFFFC );
                data[ 1 ] = type;
                data[ 2 ] = null;
                data[ 3 ] = null;
                *mPreviousNode = node;

                return node;
            }
        }

        public void Suspend()
        {
        }

        public void Resume()
        {
        }

        public void Unload()
        {
        }

        public bool CanUnload()
        {
            return false;
        }

        public bool CanSuspend()
        {
            return false;
        }

        public Action Disposing => null;
    }
}