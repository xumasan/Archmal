using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Archmal
{
    public static partial class Eval
    {
        static string NewEvalFilePath = @"./eval/NEW_KKPP.bin";
        
        public static void Clear()
        {
            Array.Clear(KKPP, 0, 12 * 12 * (int)EvalIndex.FE_END * (int)EvalIndex.FE_END);
        }

        public static void FVFileCreate()
		{
			using (var write = new BinaryWriter(File.OpenWrite(NewEvalFilePath)))
			{
				for (int i = 0; i < 12; ++i) 
                    for (int j = 0; j < 12; ++j)
                        for (int k = 0; k < (int)EvalIndex.FE_END; ++k) 
                            for (int l = 0; l < (int)EvalIndex.FE_END; ++l) 
                                write.Write(KKPP[i, j, k, l]);
			}
		}
    }
}