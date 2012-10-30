/*
 * Copied from http://msdn.microsoft.com/en-us/magazine/cc163367.aspx, Timestamp 20121027T0013
MICROSOFT LIMITED PUBLIC LICENSE

This license governs use of code marked as “sample” or “example” available on this web site without a license agreement, as provided under the section above titled “NOTICE SPECIFIC TO SOFTWARE AVAILABLE ON THIS WEB SITE.” If you use such code (the “software”), you accept this license. If you do not accept the license, do not use the software.

1. Definitions

The terms “reproduce,” “reproduction,” “derivative works,” and “distribution” have the same meaning here as under U.S. copyright law.

A “contribution” is the original software, or any additions or changes to the software.

A “contributor” is any person that distributes its contribution under this license.

“Licensed patents” are a contributor’s patent claims that read directly on its contribution.

2. Grant of Rights

(A) Copyright Grant - Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.

(B) Patent Grant - Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any contributors’ name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.

(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution.  If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.

(E) The software is licensed “as-is.” You bear the risk of using it. The contributors give no express warranties, guarantees or conditions.  You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

(F) Platform Limitation - The licenses granted in sections 2(A) and 2(B) extend only to the software or derivative works that you create that run on a Microsoft Windows operating system product.
*/
using System;
using System.Security.Cryptography;

public class CryptoRandom : Random
{
    private RNGCryptoServiceProvider _rng =
        new RNGCryptoServiceProvider();
    private byte[] _uint32Buffer = new byte[4];

    public CryptoRandom() { }
    public CryptoRandom(Int32 ignoredSeed) { }

    public override Int32 Next()
    {
        _rng.GetBytes(_uint32Buffer);
        return BitConverter.ToInt32(_uint32Buffer, 0) & 0x7FFFFFFF;
    }

    public override Int32 Next(Int32 maxValue)
    {
        if (maxValue < 0)
            throw new ArgumentOutOfRangeException("maxValue");
        return Next(0, maxValue);
    }

    public override Int32 Next(Int32 minValue, Int32 maxValue)
    {
        if (minValue > maxValue) 
            throw new ArgumentOutOfRangeException("minValue");
        if (minValue == maxValue) return minValue;
        Int64 diff = maxValue - minValue;
        while (true)
        {
            _rng.GetBytes(_uint32Buffer);
            UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);

            Int64 max = (1 + (Int64)UInt32.MaxValue);
            Int64 remainder = max % diff;
            if (rand < max - remainder)
            {
                return (Int32)(minValue + (rand % diff));
            }
        }
    }

    public override double NextDouble()
    {
        _rng.GetBytes(_uint32Buffer);
        UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);
        return rand / (1.0 + UInt32.MaxValue);
    }

    public override void NextBytes(byte[] buffer)
    {
        if (buffer == null) throw new ArgumentNullException("buffer");
        _rng.GetBytes(buffer);
    }
}