//ş
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;

namespace SSMSEditorEnhancements {
    /// <summary>
    /// https://github.com/VsVim/VsVim/blob/d7b3e1a79a6d06cdae5e0334e09f9bbf5388e7df/Src/VsVimShared/Result.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Result<T> {
        private readonly Boolean _isSuccess;
        private readonly T _value;
        private readonly Int32 _hresult;

        public Boolean IsSuccess => _isSuccess;

        public Boolean IsError => !_isSuccess;

        // TOOD: Get rid of this.  Make it a method that says throws
        public T Value {
            get {
                if (!IsSuccess) {
                    throw new InvalidOperationException();
                }

                return _value;
            }
        }

        public Int32 HResult {
            get {
                if (IsSuccess) {
                    throw new InvalidOperationException();
                }

                return _hresult;
            }
        }

        public Result (T value) {
            _value = value;
            _isSuccess = true;
            _hresult = 0;
        }

        public Result (Int32 hresult) {
            _hresult = hresult;
            _isSuccess = false;
            _value = default(T);
        }

        public T GetValueOrDefault (T defaultValue = default(T)) {
            return IsSuccess ? Value : defaultValue;
        }

        public Boolean TryGetValue (out T value) {
            if (IsSuccess) {
                value = Value;
                return true;
            }

            value = default(T);

            return false;
        }

        public static implicit operator Result<T>(Result result) {
            return new Result<T>(hresult: result.HResult);
        }

        public static implicit operator Result<T>(T value) {
            return new Result<T>(value);
        }
    }

    public struct Result {
        private readonly Boolean _isSuccess;
        private readonly Int32 _hresult;

        public Boolean IsSuccess => _isSuccess;

        public Boolean IsError => !_isSuccess;

        public Int32 HResult {
            get {
                if (!IsError) {
                    throw new InvalidOperationException();
                }

                return _hresult;
            }
        }

        private Result (Int32 hresult) {
            _hresult = hresult;
            _isSuccess = ErrorHandler.Succeeded(hresult);
        }

        public static Result Error => new Result(VSConstants.E_FAIL);

        public static Result Success => new Result(VSConstants.S_OK);

        public static Result<T> CreateSuccess<T> (T value) {
            return new Result<T>(value);
        }

        public static Result<T> CreateSuccessNonNull<T> (T value) where T : class {
            if (value == null) {
                return Result.Error;
            }

            return new Result<T>(value);
        }

        public static Result CreateError (Int32 value) {
            return new Result(value);
        }

        public static Result CreateError (Exception ex) {
            return CreateError(Marshal.GetHRForException(ex));
        }

        public static Result<T> CreateSuccessOrError<T> (T potentialValue, Int32 hresult) {
            return ErrorHandler.Succeeded(hresult)
                ? CreateSuccess(potentialValue)
                : new Result<T>(hresult: hresult);
        }
    }
}
