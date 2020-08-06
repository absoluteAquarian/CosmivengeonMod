using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.OS;
using System;
using System.IO;

namespace CosmivengeonMod.ModEdits{
	public static class ILHelper{
		public static void CompleteLog(ILCursor c){
			int index = c.Index;

			//Get the method name
			string method = c.Method.ToString();
			method = method.Substring(method.LastIndexOf(':') + 1);
			method = method.Substring(0, method.IndexOf('('));

			//And the storage path
			string path = Platform.Current.GetStoragePath();
			path = Path.Combine(path, "Terraria", "ModLoader", "Cosmivengeon");
			Directory.CreateDirectory(path);

			FileStream file = File.Open(Path.Combine(path, $"{method}.txt"), FileMode.Create);
			using(StreamWriter writer = new StreamWriter(file)){
				writer.WriteLine(DateTime.Now.ToString("'['ddMMMyyyy '-' HH:mm:ss']'"));
				writer.WriteLine($"// ILCursor: {c.Method}");
				c.Index = 0;
				do{
					Instruction curIns = c.Instrs[c.Index];
					string operand;
					if(curIns.Operand is null)
						operand = "";
					else if(curIns.Operand is ILLabel label)
						operand = $"IL_{label.Target.Offset :X4}";
					else
						operand = curIns.Operand.ToString();

					string offset = $"IL_{curIns.Offset :X4}:";
					writer.WriteLine($"{offset, -10}{curIns.OpCode.Name} {operand}");
					c.Index++;
				}while(c.Index < c.Instrs.Count);
			}

			c.Index = index;
		}

		public static void UpdateInstructionOffsets(ILCursor c){
			var instrs = c.Instrs;
			int curOffset = 0;
			foreach(var ins in instrs){
				ins.Offset = curOffset;
				curOffset += ins.GetSize();
			}
		}
	}
}
