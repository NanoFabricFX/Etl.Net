﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FastMember;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EFCore.BulkExtensions
{
    internal class ObjectReaderEx<T> : ObjectReader where T : class
    {
        private readonly HashSet<string> shadowProperties;
        private readonly Dictionary<string, ValueConverter> convertibleProperties;
        private readonly DbContext context;
        private readonly string[] members;
        private readonly FieldInfo currentFieldInfo;
        public ObjectReaderEx(Type type, IEnumerable source, HashSet<string> shadowProperties, Dictionary<string, ValueConverter> convertibleProperties, DbContext context, params string[] members) : base(type, source, members)
        {
            this.shadowProperties = shadowProperties;
            this.convertibleProperties = convertibleProperties;
            this.context = context;
            this.members = members;
            this.currentFieldInfo = typeof(ObjectReader).GetField("current", BindingFlags.Instance | BindingFlags.NonPublic);
            var ctxType = context.GetType();
        }

        public static ObjectReader Create(IEnumerable<T> source, HashSet<string> shadowProperties, Dictionary<string, ValueConverter> convertibleProperties, DbContext context, params string[] members)
        {
            bool hasShadowProp = shadowProperties.Count > 0;
            bool hasConvertibleProperties = convertibleProperties.Keys.Count > 0;
            return (hasShadowProp || hasConvertibleProperties) ? (ObjectReader)new ObjectReaderEx<T>(typeof(T), source, shadowProperties, convertibleProperties, context, members) : ObjectReader.Create(source, members);
        }

        public override object this[string name]
        {
            get
            {
                if (shadowProperties.Contains(name))
                {
                    var current = this.currentFieldInfo.GetValue(this);
                    var etr = context.Entry(current);
                    return etr.Property(name).CurrentValue;
                }
                else if (convertibleProperties.TryGetValue(name, out var converter))
                {
                    var current = this.currentFieldInfo.GetValue(this);
                    var etr = context.Entry(current);
                    var currentValue = etr.Property(name).CurrentValue;
                    return converter.ConvertToProvider(currentValue);
                }
                return base[name];
            }
        }

        public override object this[int i]
        {
            get
            {
                var name = members[i];
                return this[name];
            }
        }
    }
}
