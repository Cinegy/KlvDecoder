# Cinegy KLV Decoder Library

Use this library to decode KLV data from MPEG transport streams. The library takes a dependency on the Cinegy Transport Stream Decoder Library (Apache 2 licensed).;

## How easy is it?

The library was designed to be simple to use and flexible. Use the Cinegy TS decoder to create packets of data from a stream or a file, and pass these packets to the KLV decoder and get results!

For example, you can use it to debug / view into the guts of an MXF file (the best example of a SMPTE KLV structure).

You can print live KLV decoding, and you can use the tool to generate input logs for 'big data' analysis (which is very cool).

## Getting the library

Just to make your life easier, we auto-build this using AppVeyor and push to NuGet - here is how we are doing right now: 

[![Build status](https://ci.appveyor.com/api/projects/status/eoveo4ndhdk66nhh?svg=true)](https://ci.appveyor.com/project/cinegy/klvdecoder)

You can check out the latest compiled binary from the master or pre-master code here:

[AppVeyor KlvDecoder Project Builder](https://ci.appveyor.com/project/cinegy/klvdecoder/build/artifacts)

Available on NuGet here:

[NuGet](https://www.nuget.org/packages/Cinegy.Klv/)
