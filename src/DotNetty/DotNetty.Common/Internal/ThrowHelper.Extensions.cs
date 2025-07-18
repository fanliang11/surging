/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;

namespace DotNetty.Common
{
    #region -- ExceptionArgument --

    /// <summary>The convention for this enum is using the argument name as the enum name</summary>
    internal enum ExceptionArgument
    {
        e,
        s,

        pi,
        fi,
        ts,

        asm,
        dst,
        src,
        seq,
        key,
        obj,
        str,
        end,

        name,
        step,
        item,
        type,
        list,
        path,
        func,
        pool,
        task,

        value,
        array,
        chars,
        delay,
        types,
        match,
        index,
        count,
        other,
        inner,
        start,
        stack,

        action,
        buffer,
        thread,
        length,
        handle,
        target,
        member,
        source,
        values,
        policy,
        offset,
        method,

        creator,
        invoker,
        feature,
        manager,
        options,
        results,
        newSize,
        builder,
        retries,

        comparer,
        executor,
        sequence,
        capacity,
        typeName,
        poolName,
        assembly,
        argArray,
        fullName,
        elements,
        typeInfo,
        maxDelay,
        minValue,
        minDelay,
        timeSpan,
        nThreads,

        separator,
        taskQueue,
        fieldInfo,
        converter,
        defaultFn,
        predicate,
        increment,
        decrement,

        collection,
        startIndex,
        delimiters,
        expression,
        returnType,
        memberInfo,

        destination,
        directories,
        dirEnumArgs,
        maxCapacity,

        instanceType,
        resourceType,
        charSequence,
        valueFactory,
        propertyInfo,

        attributeType,
        threadFactory,

        chooserFactory,
        parameterTypes,

        rejectedHandler,

        aggregatePromise,
        samplingInterval,

        qualifiedTypeName,
        assemblyPredicate,

        firstNameComponent,
        includedAssemblies,
        updateValueFactory,

        secondNameComponent,

        eventExecutorFactory,
    }

    #endregion

    #region -- ExceptionResource --

    /// <summary>The convention for this enum is using the resource name as the enum name</summary>
    internal enum ExceptionResource
    {
        Capacity_May_Not_Be_Negative,
        Value_Cannot_Be_Null,
        Value_Is_Of_Incorrect_Type,
        Dest_Array_Cannot_Be_Null,
        ArgumentOutOfRange_Index,
        ArgumentOutOfRange_NeedNonNegNum,
        ArgumentOutOfRange_Count,
    }

    #endregion

    partial class ThrowHelper
    {
        #region -- Exception --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int FromException_CompareConstant()
        {
            throw GetException();

            static Exception GetException()
            {
                return new Exception("failed to compare two different constants");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int FromException_CompareSignal()
        {
            throw GetException();

            static Exception GetException()
            {
                return new Exception("failed to compare two different signal constants");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowException_InvalidCodePoint()
        {
            throw GetException();

            static Exception GetException()
            {
                return new Exception("Invalid code point!");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowException_CodepointIsDecodedButNumberOfCharactersReadIs0OrNegative()
        {
            throw GetException();

            static Exception GetException()
            {
                return new Exception("Internal error: CodePoint is decoded but number of characters read is 0 or negative");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowException_InvalidUtf8CharacterBadlyEncoded()
        {
            throw GetException();

            static Exception GetException()
            {
                return new Exception("Invalid UTF-8 character (badly encoded)");
            }
        }

        #endregion

        #region -- ArgumentException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ArgumentException GetArgumentException_DecodeHexByte(string s, int pos)
        {
            return new ArgumentException($"invalid hex byte '{s.Substring(pos, 2)}' at index {pos} of '{s}'");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException(string name)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"'{name}' is already in use");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Positive(int value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {value} (expected: > 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Positive(long value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {value} (expected: > 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PositiveOrZero(int value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {value} (expected: >= 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PositiveOrZero(long value, ExceptionArgument argument)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{GetArgumentName(argument)}: {value} (expected: >= 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_MustBeGreaterThanZero(TimeSpan tickInterval)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{nameof(tickInterval)} must be greater than 0: {tickInterval}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_MustBeGreaterThanZero(int ticksPerWheel)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{nameof(ticksPerWheel)} must be greater than 0: {ticksPerWheel}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_MustBeGreaterThanOrEquelToZero(TimeSpan quietPeriod)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{nameof(quietPeriod)} must be greater than 0: {quietPeriod}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_MustBeGreaterThanQuietPeriod(TimeSpan timeout, TimeSpan quietPeriod)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException("timeout: " + timeout + " (expected >= quietPeriod (" + quietPeriod + "))");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_ValueDiffers()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("value differs from one backed by this handle.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_MustBeLessThanOrEqualTo()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException($"tickInterval must be less than or equal to ${int.MaxValue} ms.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidLen(int length)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"length: {length}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PriorityQueueIndex<T>(int index, T item)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"item.priorityQueueIndex(): {index} (expected: -1) + item: {item}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_NotLongEnoughToHoldOutputValueUtf16()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException(
                    message: "Argument is not long enough to hold output value.",
                    paramName: "utf16");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_NotLongEnoughToHoldOutputValueUtf8()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException(
                  message: "Argument is not long enough to hold output value.",
                  paramName: "utf8");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PeriodMustNotBeEquelToZero()
        {
            throw GetException();

            static ArgumentException GetException()
            {
                return new ArgumentException("period: 0 (expected: != 0)");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_PeriodMustBeGreaterThanZero()
        {
            throw GetArgumentException_PeriodMustBeGreaterThanZero();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Task FromArgumentException_PeriodMustBeGreaterThanZero()
        {
            return TaskUtil.FromException(GetArgumentException_PeriodMustBeGreaterThanZero());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ArgumentException GetArgumentException_PeriodMustBeGreaterThanZero()
        {
            return new ArgumentException("period: 0 (expected: > 0)");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_DelayMustBeGreaterThanZero()
        {
            throw GetArgumentException_DelayMustBeGreaterThanZero();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Task FromArgumentException_DelayMustBeGreaterThanZero()
        {
            return TaskUtil.FromException(GetArgumentException_DelayMustBeGreaterThanZero());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ArgumentException GetArgumentException_DelayMustBeGreaterThanZero()
        {
            return new ArgumentException("delay: 0 (expected: > 0)");
        }

        #endregion

        #region -- ArgumentOutOfRangeException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_AppendableCharSequence_Count(int count, int pos)
        {
            throw GetException();
            ArgumentOutOfRangeException GetException()
            {
                return new ArgumentOutOfRangeException(nameof(count), "length: " + count + " (length: >= 0, <= " + pos + ')');
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_MustBeGreaterThan(int ticksPerWheel)
        {
            throw GetException();
            ArgumentOutOfRangeException GetException()
            {
                return new ArgumentOutOfRangeException(nameof(ticksPerWheel),
                    $"{nameof(ticksPerWheel)} may not be greater than 2^30: {ticksPerWheel}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_StartIndex(ExceptionArgument argument)
        {
            throw GetArgumentOutOfRangeException();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), "StartIndex cannot be less than zero.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_EndIndexLessThanStartIndex()
        {
            throw GetArgumentOutOfRangeException();

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("end", "EndIndex cannot be less than StartIndex.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_IndexLargerThanLength(ExceptionArgument argument)
        {
            throw GetArgumentOutOfRangeException();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), $"{GetArgumentName(argument)} must be less than length of char sequence.");
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_Slice(int length, int totalLength)
        {
            throw GetArgumentOutOfRangeException();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(nameof(length), $"length({length}) cannot be longer than Array.length({totalLength})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_Slice(int index, int length, int totalLength)
        {
            throw GetArgumentOutOfRangeException();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(nameof(length), $"index: ({index}), length({length}) index + length cannot be longer than Array.length({totalLength})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_SetRange_Index(int index, int srcLength, int totalLength)
        {
            throw GetArgumentOutOfRangeException();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(nameof(srcLength), $"index: ({index}), srcLength({srcLength}) index + length cannot be longer than Array.length({totalLength})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_SetRange_SrcIndex(int srcIndex, int srcLength, int totalLength)
        {
            throw GetArgumentOutOfRangeException();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(nameof(srcLength), $"index: ({srcIndex}), srcLength({srcLength}) index + length cannot be longer than src.length({totalLength})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_LogLevel(LogLevel logLevel)
        {
            throw GetArgumentOutOfRangeException();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ArgumentOutOfRangeException_InvalidUnicodeChar()
        {
            throw GetArgumentOutOfRangeException();

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(
                   message: "Value must be between U+0000 and U+D7FF, inclusive; or value must be between U+E000 and U+FFFF, inclusive.",
                   paramName: "char");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ArgumentOutOfRangeException_InvalidUnicodeValue()
        {
            throw GetArgumentOutOfRangeException();

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(
                    message: "Value must be between U+0000 and U+D7FF, inclusive; or value must be between U+E000 and U+10FFFF, inclusive.",
                    paramName: "value");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ArgumentOutOfRangeException_Positive(TimeSpan timeSpan, ExceptionArgument argument)
        {
            throw GetArgumentOutOfRangeException();

            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), timeSpan, $"{GetArgumentName(argument)} must be a positive number.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ArgumentOutOfRangeException_NextTimeSpan_Positive(TimeSpan timeSpan, ExceptionArgument argument)
        {
            throw GetArgumentOutOfRangeException();

            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(GetArgumentName(argument), timeSpan, $"SafeRandom.NextTimeSpan {GetArgumentName(argument)} must be a positive number.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ArgumentOutOfRangeException_NextTimeSpan_minValue(TimeSpan minValue)
        {
            throw GetArgumentOutOfRangeException();

            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("minValue", minValue, "SafeRandom.NextTimeSpan minValue must be greater than maxValue.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ArgumentOutOfRangeException_Invalid_minValue(TimeSpan minValue)
        {
            throw GetArgumentOutOfRangeException();

            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("minValue", minValue, "max delay must be greater than min delay.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ArgumentOutOfRangeException_Invalid_minValue(TimeSpan _minDelay, TimeSpan currMax)
        {
            throw GetArgumentOutOfRangeException();

            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException($"minDelay {_minDelay}, currMax = {currMax}");
            }
        }

        #endregion

        #region -- InvalidOperationException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_CannotBeCalledFromTimerTask()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException($"{nameof(HashedWheelTimer)}.stop() cannot be called from timer task.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_InvalidWorkerState()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Invalid WorkerState");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_CannotBeStartedOnceStopped()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("cannot be started once stopped");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_EnumeratorNotInit()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Enumerator not initialized.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_EnumeratorAlreadyCompleted()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Eumerator already completed.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_TooMany()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("too many thread-local indexed variables");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_RecycledAlready()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("recycled already");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_ReleasedAlready()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("released already");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_AlreadyFinished()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Already finished");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_MustBeCalledFromEventexecutorThread()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Must be called from EventExecutor thread");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_AddingPromisesIsNotAllowedAfterFinishedAdding()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Adding promises is not allowed after finished adding");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_RecycledMultiTimes()
        {
            throw GetException();

            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("recycled multiple times");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_CapacityMustBePositive(int newCapacity)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"New capacity {newCapacity} must be positive");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Unexpected(Signal signal)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"unexpected signal: {signal}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Deadline(TimeSpan timeoutDeadline, TimeSpan deadline)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException(
                    string.Format("timeout.deadline {0} > deadline {1}", timeoutDeadline, deadline));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_EnumeratorIsOnInvalidPosition()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("Enumerator is on invalid position");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_InvalidCharactersInTheString()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("Invalid characters in the string");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_MovenextNeedsToBeCalledAtLeastOnce()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("MoveNext() needs to be called at least once");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_CurrentDoesNotExist()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("Current does not exist");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Must_be_invoked_from_an_event_loop()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("must be invoked from an event loop");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Cannot_await_termination_of_the_current_thread()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("cannot await termination of the current thread");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_FailedToCreateAChildEventLoop(Exception e)
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("failed to create a child event loop.", e);
            }
        }

        #endregion

        #region -- IndexOutOfRangeException --

        internal static void ThrowIndexOutOfRangeException_Start(int start, int length, int count)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("expected: 0 <= start({0}) <= start + length({1}) <= value.length({2})", start, length, count));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_Index(int index, int length, int capacity)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("index: {0}, length: {1} (expected: range(0, {2}))", index, length, capacity));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRangeException_ParseChar(int start)
        {
            throw GetArgumentOutOfRangeException();
            IndexOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new IndexOutOfRangeException($"2 bytes required to convert to character. index {start} would go out of bounds.");
            }
        }

        #endregion

        #region -- FormatException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowFormatException()
        {
            throw GetException();

            static FormatException GetException()
            {
                return new FormatException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowFormatException_Radix()
        {
            throw GetException();

            static FormatException GetException()
            {
                return new FormatException($"Radix must be from {CharUtil.MinRadix} to {CharUtil.MaxRadix}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowFormatException(ICharSequence seq, int start, int end)
        {
            throw GetException();
            FormatException GetException()
            {
                return new FormatException(seq.SubSequence(start, end).ToString());
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowFormatException(int start, int end)
        {
            throw GetException();
            FormatException GetException()
            {
                return new FormatException($"Content is empty because {start} and {end} are the same.");
            }
        }

        #endregion

        #region -- RejectedExecutionException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static RejectedExecutionException GetRejectedExecutionException()
        {
            return new RejectedExecutionException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int ThrowRejectedExecutionException()
        {
            throw GetRejectedExecutionException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int ThrowRejectedExecutionException_TimerStopped()
        {
            throw GetException();

            static RejectedExecutionException GetException()
            {
                return new RejectedExecutionException("Timer has been stopped and cannot process new operations.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int ThrowRejectedExecutionException_NumOfPendingTimeouts(long pendingTimeoutsCount, long maxPendingTimeouts)
        {
            throw GetException();
            RejectedExecutionException GetException()
            {
                return new RejectedExecutionException($"Number of pending timeouts ({pendingTimeoutsCount}) is greater than or equal to maximum allowed pending timeouts ({maxPendingTimeouts})");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowRejectedExecutionException_Terminated()
        {
            throw GetSocketException();

            static RejectedExecutionException GetSocketException()
            {
                return new RejectedExecutionException($"{nameof(SingleThreadEventExecutor)} terminated");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowRejectedExecutionException_Shutdown()
        {
            throw GetSocketException();

            static RejectedExecutionException GetSocketException()
            {
                return new RejectedExecutionException($"{nameof(SingleThreadEventExecutor)} already shutdown");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowRejectedExecutionException_Queue()
        {
            throw GetSocketException();

            static RejectedExecutionException GetSocketException()
            {
                return new RejectedExecutionException($"{nameof(SingleThreadEventExecutor)} queue task failed");
            }
        }

        #endregion

        #region -- TimeoutException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static TimeoutException GetTimeoutException_WithTimeout(TimeSpan timeout)
        {
            return new TimeoutException($"WithTimeout has timed out after {timeout}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowTimeoutException_WithTimeout(TimeSpan timeout)
        {
            throw GetTimeoutException_WithTimeout(timeout);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowTimeoutException_WaitWithThrow(TimeSpan timeout)
        {
            throw GetException();

            TimeoutException GetException()
            {
                return new TimeoutException($"Task.WaitWithThrow has timed out after {timeout}.");
            }
        }

        #endregion

        #region -- NotSupportedException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NotSupportedException GetNotSupportedException()
        {
            return new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NotSupportedException ThrowNotSupportedException()
        {
            return GetNotSupportedException();
        }

        #endregion

        #region -- OverflowException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static OverflowException GetOverflowException()
        {
            return new OverflowException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowOverflowException()
        {
            throw GetOverflowException();
        }

        #endregion
    }
}
