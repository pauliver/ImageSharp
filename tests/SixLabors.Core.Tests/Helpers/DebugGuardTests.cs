﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

// tell this file to enable debug conditional method calls, i.e. all the debug guard calls
#define DEBUG

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SixLabors.Helpers.Tests
{
    public class DebugGuardTests
    {
        [Fact]
        public void AllStaticMethodsOnOnDebugGuardHaveDEBUGConditional()
        {
            var methods = typeof(DebugGuard).GetTypeInfo().GetMethods()
                .Where(x => x.IsStatic);

            foreach (var m in methods)
            {
                var attribs = m.GetCustomAttributes<ConditionalAttribute>();
                Assert.True(attribs.Select(x => x.ConditionString).Contains("DEBUG"), $"Method '{m.Name}' does not have [Conditional(\"DEBUG\")] set.");
            }
        }

        [Fact]
        public void NotNull_TargetNotNull_ThrowsNoException()
        {
            DebugGuard.NotNull("test", "myParamName");
        }

        [Fact]
        public void NotNull_TargetNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                DebugGuard.NotNull((object)null, "myParamName");
            });
        }

        [Fact]
        public void MustBeLessThan_IsLess_ThrowsNoException()
        {
            DebugGuard.MustBeLessThan(0, 1, "myParamName");
        }

        [Theory]
        [InlineData(2, 1)]
        [InlineData(1, 1)]
        public void MustBeLessThan_IsGreaterOrEqual_ThrowsNoException(int value, int max)
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                DebugGuard.MustBeLessThan(value, max, "myParamName");
            });

            Assert.Equal("myParamName", exception.ParamName);
            Assert.Contains($"Value must be less than {max}.", exception.Message);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        public void MustBeLessThanOrEqualTo_IsLessOrEqual_ThrowsNoException(int value, int max)
        {
            DebugGuard.MustBeLessThanOrEqualTo(value, max, "myParamName");
        }

        [Fact]
        public void MustBeLessThanOrEqualTo_IsGreater_ThrowsNoException()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                DebugGuard.MustBeLessThanOrEqualTo(2, 1, "myParamName");
            });

            Assert.Equal("myParamName", exception.ParamName);
            Assert.Contains($"Value must be less than or equal to 1.", exception.Message);
        }

        [Fact]
        public void MustBeGreaterThan_IsGreater_ThrowsNoException()
        {
            DebugGuard.MustBeGreaterThan(2, 1, "myParamName");
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(1, 1)]
        public void MustBeGreaterThan_IsLessOrEqual_ThrowsNoException(int value, int min)
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                DebugGuard.MustBeGreaterThan(value, min, "myParamName");
            });

            Assert.Equal("myParamName", exception.ParamName);
            Assert.Contains($"Value must be greater than {min}.", exception.Message);
        }

        [Theory]
        [InlineData(2, 1)]
        [InlineData(1, 1)]
        public void MustBeGreaterThanOrEqualTo_IsGreaterOrEqual_ThrowsNoException(int value, int min)
        {
            DebugGuard.MustBeGreaterThanOrEqualTo(value, min, "myParamName");
        }

        [Fact]
        public void MustBeGreaterThanOrEqualTo_IsLess_ThrowsNoException()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                DebugGuard.MustBeGreaterThanOrEqualTo(1, 2, "myParamName");
            });

            Assert.Equal("myParamName", exception.ParamName);
            Assert.Contains($"Value must be greater than or equal to 2.", exception.Message);
        }

        [Theory]
        [InlineData(new int[] { 1, 2 }, 1)]
        [InlineData(new int[] { 1, 2 }, 2)]
        public void MustBeSizedAtLeast_Array_LengthIsGreaterOrEqual_ThrowsNoException(int[] value, int minLength)
        {
            DebugGuard.MustBeSizedAtLeast<int>(value, minLength, "myParamName");
        }

        [Fact]
        public void MustBeSizedAtLeast_Array_LengthIsLess_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                DebugGuard.MustBeSizedAtLeast<int>(new int[] { 1, 2 }, 3, "myParamName");
            });

            Assert.Equal("myParamName", exception.ParamName);
            Assert.Contains($"The size must be at least 3.", exception.Message);
        }
    }
}
