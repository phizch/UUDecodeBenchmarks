# UUEncoding Benchmarks

This repository contains my journey of improving the performance of decoding
a block of UUEncoded (Unix-to-Unix Encoding) text to 8-bit data.

Although it shows the algorithm for decoding the 6-bit (base-64) encoded text to 8-bit binary, 
this is not a complete implementation of the UUEncoding scheme. 

**It should not be used for anything more than a reference.**

I started out with various scalar ways of decoding 4-byte blocks, and then went on to try my hands on vectorization. 
The fastest implementations are a 256 bit Avx2 version, and an unrolled 2x128 bit Sse2/Ssse3 version.


#### Benchmarks

The benchmarks are in the order of implementation, though some of them are identical.

Initially, I didn't use `[MethodImpl( MethodImplOptions.AggressiveInlining )]`, 
but I chose to do it for all the methods here. 
This means `Decode16Bytes_v10` and `Decode16Bytes_v10_Inline` are identical. 

##### Known issues
- There are no checks whether the CPU supports the instructions needed. Use with care.

#### License: MIT

*Thanks to the people on Discord who's helped me.*