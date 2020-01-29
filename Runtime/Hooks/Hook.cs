using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Allows replacing any static method with another static method
/// Instance methods are not supported, unless an instance method is replaced with a static method that has a first parameter type equal to the instance type.
/// This is because argument number 0 is the this parameter in an instance method, and is the first method arg in a static method.
/// When compiling, accessing argument 0 in an instance method emits ldarg_1, instead of ldarg_0. This means that arguments are shifted left by one and the first arg is discarded.
/// NOTE: Lambdas and delegates classify as instance methods, because the compiler emits a class with an instance method when compiling a delegate. Ask me for more details.
/// </summary>
public class Hook
{
    public bool Valid { get; } // Is the hook valid?

    readonly IntPtr m_targetPtr; // Pointer to the target function
    readonly byte[] m_original; // Backup of the original target method
    readonly NativeDetourData m_detour; // Detour data

    public Hook(Delegate target, Delegate hook)
    {
        // Cannot hook a function with a different signature
        if (target.GetType() != hook.GetType())
        {
            Valid = false;
            return;
        }

        // Literally don't understand wtf this does, but it does something good i think...
        RuntimeHelpers.PrepareMethod(hook.Method.MethodHandle);
        RuntimeHelpers.PrepareMethod(target.Method.MethodHandle);

        // Get pointers to the functions
        m_targetPtr = target.Method.MethodHandle.GetFunctionPointer();
        var hookPtr = hook.Method.MethodHandle.GetFunctionPointer();

        // Create a native detour. Writes a trampoline to the method at m_targetPtr, and makes execution jump directly to hookPtr
        m_detour = DetourHelper.Native.Create(m_targetPtr, hookPtr);

        // Backup the original assembly code
        m_original = new byte[m_detour.Size];
        Marshal.Copy(m_targetPtr, m_original, 0, m_original.Length);

        // Success! We have a valid hook
        Valid = true;
    }

    /// <summary>
    /// Apply the hook to the target method.
    /// Does nothing if the hook is invalid.
    /// </summary>
    public void Install()
    {
        if (!Valid) return;

        // Write the detour to the target method
        DetourHelper.Native.Apply(m_detour);
    }

    public void Uninstall()
    {
        if (!Valid) return;

        // Restore the target method to the backup created in the constructor
        Marshal.Copy(m_original, 0, m_targetPtr, m_original.Length);
    }
}

public class HookCollection : ICollection<Hook>
{
    readonly List<Hook> m_hooks;

    public HookCollection()
    {
        m_hooks = new List<Hook>();
    }

    public void Hook(Delegate target, Delegate hook, bool autoInstall = true)
    {
        var newHook = new Hook(target, hook);
        if (autoInstall) newHook.Install();
        m_hooks.Add(newHook);
    }

    public void InstallAll() => m_hooks.ForEach(h => h.Install());
    public void UninstallAll() => m_hooks.ForEach(h => h.Uninstall());

    #region ICollection delegation to m_hooks
    public IEnumerator<Hook> GetEnumerator()
    {
        return m_hooks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) m_hooks).GetEnumerator();
    }

    public void Add(Hook item)
    {
        m_hooks.Add(item);
    }

    public void Clear()
    {
        m_hooks.Clear();
    }

    public bool Contains(Hook item)
    {
        return m_hooks.Contains(item);
    }

    public void CopyTo(Hook[] array, int arrayIndex)
    {
        m_hooks.CopyTo(array, arrayIndex);
    }

    public bool Remove(Hook item)
    {
        return m_hooks.Remove(item);
    }

    public int Count => m_hooks.Count;

    public bool IsReadOnly => ((ICollection<Hook>) m_hooks).IsReadOnly;
    #endregion
}