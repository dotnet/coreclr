// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/*============================================================
**
**
**
** Purpose: A random number generator.
**
** 
===========================================================*/

namespace System {
    using Diagnostics.Contracts;
    [Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public class Random {

        //
        // Private Constants 
        //

        private const double REAL_UNIT_INT = 1.0 / ( int.MaxValue + 1.0 );
        private const double REAL_UNIT_UINT = 1.0 / ( uint.MaxValue + 1.0 );
        private const uint Y = 842502087;
        private const uint Z = 3579807591;
        private const uint W = 273326509;

        //
        // Member Variables
        //

        private uint _x;
        private uint _y;
        private uint _z;
        private uint _w;

        //
        // Constructors
        //

        public Random() : this(Environment.TickCount) { }
    
        public Random(int Seed) {
            Reset( Seed );
        }

        private void Reset( int seed ) {
            _x = (uint)seed;
            _y = Y;
            _z = Z;
            _w = W;
        }

        //
        // Package Private Methods
        //

        /*====================================Sample====================================
        **Action: Return a new random number [0..1) and reSeed the Seed array.
        **Returns: A double [0..1)
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/

        protected virtual double Sample() {
            uint t = _x ^ _x << 11;
            _x = _y; _y = _z; _z = _w;
            return REAL_UNIT_INT * (int)( 0x7FFFFFFF & ( _w = _w ^ _w >> 19 ^ ( t ^ t >> 8 ) ) );
        }

        //
        // Public Instance Methods
        //

        /*=====================================Next=====================================
        **Returns: An int [0..Int32.MaxValue)
        **Arguments: None
        **Exceptions: None.
        ==============================================================================*/

        public virtual int Next() {
            uint t = _x ^ _x << 11;
            _x = _y;
            _y = _z;
            _z = _w;
            _w = _w ^ _w >> 19 ^ ( t ^ t >> 8 );
            uint rtn = _w & 0x7FFFFFFF;
            return (int)rtn;
        }

        /*=====================================Next=====================================
        **Returns: An int [minvalue..maxvalue)
        **Arguments: minValue -- the least legal value for the Random number.
        **           maxValue -- One greater than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/

        public virtual int Next(int minValue, int maxValue) {
            if (minValue>maxValue) {
                throw new ArgumentOutOfRangeException("minValue",Environment.GetResourceString("Argument_MinMaxValue", "minValue", "maxValue"));
            }
            Contract.EndContractBlock();
            uint t = _x ^ _x << 11;
            _x = _y; _y = _z; _z = _w;
            int range = maxValue - minValue;
            if ( range < 0 ) {
                // If range is <0 then an overflow has occured and must resort to using long integer arithmetic instead (slower).
                return minValue + (int)( REAL_UNIT_UINT * ( _w = _w ^ _w >> 19 ^ ( t ^ t >> 8 ) ) * ( (long)maxValue - minValue ) );
            }
            return minValue + (int)( REAL_UNIT_INT * (int)( 0x7FFFFFFF & ( _w = ( _w ^ _w >> 19 ) ^ ( t ^ t >> 8 ) ) ) * range );
        }

        /*=====================================Next=====================================
        **Returns: An int [0..maxValue)
        **Arguments: maxValue -- One more than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/

        public virtual int Next(int maxValue) {
            if (maxValue<0) {
                throw new ArgumentOutOfRangeException("maxValue", Environment.GetResourceString("ArgumentOutOfRange_MustBePositive", "maxValue"));
            }
            Contract.EndContractBlock();
            uint t = _x ^ _x << 11;
            _x = _y; _y = _z; _z = _w;
            return (int)( REAL_UNIT_INT * (int)( 0x7FFFFFFF & ( _w = _w ^ _w >> 19 ^ ( t ^ t >> 8 ) ) ) * maxValue );
        }

        /*=====================================Next=====================================
        **Returns: A double [0..1)
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/

        public virtual double NextDouble() {
            return Sample();
        }

        /*==================================NextBytes===================================
        **Action:  Fills the byte array with random bytes [0..0x7f].  The entire array is filled.
        **Returns:Void
        **Arugments:  buffer -- the array to be filled.
        **Exceptions: None
        ==============================================================================*/

        public unsafe virtual void NextBytes( byte[] buffer ) {
            if ( buffer == null ) throw new ArgumentNullException( "buffer" );
            Contract.EndContractBlock();

            uint x = _x, y = _y, z = _z, w = _w;
            int i = 0;
            uint t;
            if ( buffer.Length > sizeof(int) - 1 ) {
                fixed (byte* bptr = buffer)
                {
                    uint* iptr = (uint*)bptr;
                    uint* endptr = iptr + buffer.Length / 4;
                    do {
                        t = ( x ^ ( x << 11 ) );
                        x = y; y = z; z = w;
                        w = w ^ w >> 19 ^ ( t ^ t >> 8 );
                        *iptr = w;
                    }
                    while ( ++iptr < endptr );
                    i = buffer.Length - buffer.Length % 4;
                }
            }
            // Fill up any remaining bytes in the buffer.
            if ( i < buffer.Length ) {
                t = ( x ^ ( x << 11 ) );
                x = y; y = z; z = w;
                w = w ^ w >> 19 ^ ( t ^ t >> 8 );
                do {
                    buffer[ i ] = (byte)( w >>= 8 );
                } while ( ++i < buffer.Length );
            }
            _x = x; _y = y; _z = z; _w = w;
        }
    }
}
