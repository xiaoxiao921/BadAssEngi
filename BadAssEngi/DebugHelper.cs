using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API.Utils;
using UnityEngine;
using SR = System.Reflection;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace BadAssEngi
{
    internal static class DebugHelper
    {
        internal static void Init()
        {
            var _ = new ILHook(typeof(StackTraceUtility).GetMethodCached("ExtractStringFromExceptionpublic"),
                GiveFaultyIlLines);
            Debug.LogWarning("DebugHelper is active");
        }

        private static void GiveFaultyIlLines(ILContext il)
        {
            var c = new ILCursor(il);

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<object>>(ex =>
            {
                var trace = new System.Diagnostics.StackTrace((Exception)ex);
                var frame = trace.GetFrame(0);

                var c2 = new ILCursor(new ILContext(frame.GetMethod().ToDefinition()));

                var faultyIlOffset = frame.GetILOffset();
                for (var i = 0; i < c2.Instrs.Count; i++)
                {
                    var instruction = c2.Instrs[i];

                    if (instruction.Offset == faultyIlOffset)
                    {
                        var firstInstrIndex = i - 3;
                        firstInstrIndex = firstInstrIndex <= 0 ? 0 : firstInstrIndex;
                        var lastInstrIndex = i + 3;
                        lastInstrIndex = lastInstrIndex >= c2.Instrs.Count ? c2.Instrs.Count : lastInstrIndex;

                        for (int j = firstInstrIndex; j <= lastInstrIndex; j++)
                        {
                            var instrToString = c2.Instrs[j].ToString();

                            if (j == i)
                                Debug.LogWarning(instrToString + " <--- Faulty");
                            else
                                Debug.LogWarning(instrToString);
                        }
                    }
                }
            });
        }
    }

    internal static class DebugCecilExtensions
    {
        internal static TypeDefinition ToDefinition(this Type self)
        {
            var module = ModuleDefinition.ReadModule(new MemoryStream(File.ReadAllBytes(self.Module.FullyQualifiedName)));
            return (TypeDefinition)module.LookupToken(self.MetadataToken);
        }

        internal static MethodDefinition ToDefinition(this SR.MethodBase method)
        {
            var declaringType = method.DeclaringType.ToDefinition();
            return (MethodDefinition)declaringType.Module.LookupToken(method.MetadataToken);
        }
    }
}