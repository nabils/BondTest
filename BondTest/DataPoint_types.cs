
//------------------------------------------------------------------------------
// This code was generated by a tool.
//
//   Tool : Bond Compiler 3.02
//   File : DataPoint_types.cs
//
// Changes to this file may cause incorrect behavior and will be lost when
// the code is regenerated.
// <auto-generated />
//------------------------------------------------------------------------------


#region ReSharper warnings
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable RedundantUsingDirective
#endregion

namespace BondTest
{
    using System.Collections.Generic;

    [global::Bond.Schema]
    [System.CodeDom.Compiler.GeneratedCode("gbc", "3.02")]
    public partial class Data
    {
        [global::Bond.Id(0)]
        public string Symbol { get; set; }

        [global::Bond.Id(1)]
        public double Delta { get; set; }

        [global::Bond.Id(2)]
        public long TimeStamp { get; set; }
        
        public Data()
            : this("BondTest.DataPoint", "DataPoint")
        {}

        protected Data(string fullName, string name)
        {
            Symbol = string.Empty;
        }
    }
} // BondTest
