// Lifted from NATS.NKeys project
// https://github.com/nats-io/nkeys.net/blob/main/NATS.NKeys/Base32.cs

using System.Security.Cryptography;

namespace RickDotNet.Base.Utils;

using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Base32 encoding and decoding.
/// </summary>
public static class Base32
{
    /// <summary>
    /// Decodes a Base32-encoded string into a byte array.
    /// </summary>
    /// <param name="encoded">The Base32-encoded string to decode.</param>
    /// <param name="result">The byte array to store the decoded data.</param>
    /// <returns>The number of bytes decoded.</returns>
    public static int FromBase32(ReadOnlySpan<char> encoded, Span<byte> result)
    {
        var currentByte = 0;
        var bitsRemaining = 8;
        var outputLength = 0;

        foreach (var currentChar in encoded)
        {
            if (currentChar is >= 'A' and <= 'Z')
                currentByte = (currentByte << 5) | (currentChar - 'A');
            else if (currentChar is >= '2' and <= '7')
                currentByte = (currentByte << 5) | (currentChar - '2' + 26);
            else if (currentChar == '=')
                continue;
            else
                ThrowInvalidBase32CharacterException();

            bitsRemaining -= 5;
            if (bitsRemaining <= 0)
            {
                result[outputLength++] = (byte)(currentByte >> -bitsRemaining);
                currentByte &= (1 << -bitsRemaining) - 1;
                bitsRemaining += 8;
            }
        }

        return outputLength;
    }

    /// <summary>
    /// Converts a byte array to a Base32-encoded string.
    /// </summary>
    /// <param name="data">The byte array to encode.</param>
    /// <param name="output">The span of characters to store the encoded data.</param>
    /// <returns>The number of characters encoded.</returns>
    public static int ToBase32(ReadOnlySpan<byte> data, Span<char> output)
    {
        var outputLen = GetEncodedLength(data);
        if (output.Length < outputLen)
            ThrowInsufficientSpaceException();

        var buffer = 0;
        var bufferBits = 0;
        var outputIndex = 0;

        var base32 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"u8;
        var charsPtr = output;
        var dataPtr = data;
        for (var i = 0; i < data.Length; i++)
        {
            buffer = (buffer << 8) | dataPtr[i];
            bufferBits += 8;

            while (bufferBits >= 5)
            {
                charsPtr[outputIndex++] = (char)base32[(buffer >> (bufferBits - 5)) & 0x1F];
                bufferBits -= 5;
            }
        }

        if (bufferBits > 0)
        {
            charsPtr[outputIndex++] = (char)base32[(buffer << (5 - bufferBits)) & 0x1F];
        }

        return outputIndex;
    }
    
    /// <summary>
    /// Converts a byte array to a Base32-encoded string.
    /// </summary>
    /// <param name="data">The byte array to encode.</param>
    /// <returns>A Base32-encoded string.</returns>
    public static string ToBase32(ReadOnlySpan<byte> data)
    {
        var encodedLength = GetEncodedLength(data);
        Span<char> output = stackalloc char[encodedLength];
        
        ToBase32(data, output);
        return output.ToString();
    }
    
    public static string RandomBase32(int length)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");
        
        // length param is for encoded length
        // base32 is 5bits per char instead
        // of 8bits per byte
        var dataLength = length * 5 / 8;
        byte[] randomBytes = new byte[dataLength];
        RandomNumberGenerator.Fill(randomBytes);
        
        //var encodedLength = GetEncodedLength(dataLength);
        var base32Chars = new char[length];
        ToBase32(randomBytes, base32Chars);
        
        return new string(base32Chars);
    }

    /// <summary>
    /// Calculates the length of the decoded data from a Base32-encoded string.
    /// </summary>
    /// <param name="encoded">The Base32-encoded string.</param>
    /// <returns>The length of the decoded data.</returns>
    public static int GetDataLength(ReadOnlySpan<char> encoded)
    {
        var length = 0;
        foreach (var currentChar in encoded)
        {
            if (currentChar is >= 'A' and <= 'Z' or >= '2' and <= '7')
                length++;
            else if (currentChar != '=')
                ThrowInvalidBase32CharacterException();
        }

        return length * 5 / 8;
    }

    /// <summary>
    /// Calculates the length of the Base32 encoding for a given byte array.
    /// </summary>
    /// <param name="data">The byte array to calculate the encoding length for.</param>
    /// <returns>The length of the Base32 encoding.</returns>
    private static int GetEncodedLength(ReadOnlySpan<byte> data)
    {
        var bitsCount = data.Length * 8;
        var rem = bitsCount % 5;
        if (rem > 0)
        {
            bitsCount += 5 - rem;
        }

        var outputLen = bitsCount / 5;

        return outputLen;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInsufficientSpaceException() => throw new ArgumentException("Insufficient space in output buffer");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidBase32CharacterException() => throw new ArgumentException("Invalid base32 character");
}