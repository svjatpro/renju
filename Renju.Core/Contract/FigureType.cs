﻿namespace Renju.Core;

public enum FigureType
{
    None = 0x00,
    ClosedTwo = 0x01,     // Closed vector of two stones 11
    ClosedTwo1 = 0x02,    // Closed vector of two stones 101
    ClosedTwo2 = 0x03,    // Closed vector of two stones 1001
    ClosedTwo3 = 0x04,    // Closed vector of two stones 10001
    OpenTwo = 0x05,       // Opened vector of two stones
    ClosedThree  = 0x06,  // Closed vector of three stones 111
    ClosedThree1 = 0x07,  // Closed vector of three stones 1011
    ClosedThree2 = 0x08,  // Closed vector of three stones 10101
    OpenThree = 0x09,     // Opened vector of three stones
    ClosedFour = 0x0a,    // Closed vector of four stones
    OpenFour = 0x0b,      // Opened vector of four stones
    SixOrMore = 0x0c,     // vector of six or more stones
    Five = 0x0d,          // vector of five stones
}