//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Security;
using System.Security.Principal;

#endregion

namespace CassiniDev
{
    [SuppressUnmanagedCodeSecurity]
    public sealed class NtlmAuth : IDisposable
    {
        
        public readonly bool _credentialsHandleAcquired;

        public string _blob;

        public bool _completed;

        public NativeMethods.SecHandle _credentialsHandle;

        public NativeMethods.SecBuffer _inputBuffer;
        public NativeMethods.SecBufferDesc _inputBufferDesc;

        public NativeMethods.SecBuffer _outputBuffer;

        public NativeMethods.SecBufferDesc _outputBufferDesc;

        public NativeMethods.SecHandle _securityContext;

        public bool _securityContextAcquired;

        public uint _securityContextAttributes;

        public SecurityIdentifier _sid;

        public long _timestamp;

        public IntPtr _phToken;

        public NtlmAuth()
        {
            if (
                NativeMethods.AcquireCredentialsHandle(null, "NTLM", 1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                                                 ref _credentialsHandle, ref _timestamp) != 0)
            {
                throw new InvalidOperationException();
            }
            _credentialsHandleAcquired = true;
        }

        public string Blob
        {
            get { return _blob; }
        }

        public bool Completed
        {
            get { return _completed; }
        }

        public SecurityIdentifier SID
        {
            get { return _sid; }
        }

        public IntPtr Token
        {
            get { return _phToken; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            FreeUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        #endregion

        public unsafe bool Authenticate(string blobString)
        {
            _blob = null;
            byte[] buffer = Convert.FromBase64String(blobString);
            byte[] inArray = new byte[0x4000];
            fixed (void* ptrRef = &_securityContext)
            {
                fixed (void* ptrRef2 = &_inputBuffer)
                {
                    fixed (void* ptrRef3 = &_outputBuffer)
                    {
                        fixed (void* ptrRef4 = buffer)
                        {
                            fixed (void* ptrRef5 = inArray)
                            {
                                IntPtr zero = IntPtr.Zero;
                                if (_securityContextAcquired)
                                {
                                    zero = (IntPtr)ptrRef;
                                }
                                _inputBufferDesc.ulVersion = 0;
                                _inputBufferDesc.cBuffers = 1;
                                _inputBufferDesc.pBuffers = (IntPtr)ptrRef2;
                                _inputBuffer.cbBuffer = (uint)buffer.Length;
                                _inputBuffer.BufferType = 2;
                                _inputBuffer.pvBuffer = (IntPtr)ptrRef4;
                                _outputBufferDesc.ulVersion = 0;
                                _outputBufferDesc.cBuffers = 1;
                                _outputBufferDesc.pBuffers = (IntPtr)ptrRef3;
                                _outputBuffer.cbBuffer = (uint)inArray.Length;
                                _outputBuffer.BufferType = 2;
                                _outputBuffer.pvBuffer = (IntPtr)ptrRef5;
                                int num = NativeMethods.AcceptSecurityContext(ref _credentialsHandle, zero,
                                                                        ref _inputBufferDesc, 20,
                                                                        0, ref _securityContext, ref _outputBufferDesc,
                                                                        ref _securityContextAttributes, ref _timestamp);
                                if (num == 0x90312)
                                {
                                    _securityContextAcquired = true;
                                    _blob = Convert.ToBase64String(inArray, 0, (int)_outputBuffer.cbBuffer);
                                }
                                else
                                {
                                    if (num != 0)
                                    {
                                        return false;
                                    }
                                    _phToken = IntPtr.Zero;
                                    if (NativeMethods.QuerySecurityContextToken(ref _securityContext, ref _phToken) != 0)
                                    {
                                        return false;
                                    }

                                    using (WindowsIdentity identity = new WindowsIdentity(_phToken))
                                    {
                                        _sid = identity.User;
                                    }

                                    _completed = true;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        ~NtlmAuth()
        {
            FreeUnmanagedResources();
        }

        public void FreeUnmanagedResources()
        {
            if (_phToken != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(_phToken);
                _phToken = IntPtr.Zero;

            }

            if (_securityContextAcquired)
            {
                NativeMethods.DeleteSecurityContext(ref _securityContext);
            }
            if (_credentialsHandleAcquired)
            {
                NativeMethods.FreeCredentialsHandle(ref _credentialsHandle);
            }
        }
    }
}