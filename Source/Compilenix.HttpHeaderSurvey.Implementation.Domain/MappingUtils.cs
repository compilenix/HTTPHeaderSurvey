using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.Domain.DataTransferObjects;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class MappingUtils
    {
        private static bool IsMappingInitialized { get; set; }

        [NotNull]
        public static TTarget Map<TTarget>([NotNull] object source)
        {
            return Mapper.Map<TTarget>(source);
        }

        [NotNull]
        public static TTarget Map<TTarget>([NotNull] object source, [NotNull] object target)
            where TTarget : class
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Mapper.Map(source, target) as TTarget;
        }

        [NotNull]
        public static IEnumerable<TTarget> MapRange<TTarget>([ItemNotNull] [NotNull] IEnumerable<object> sources)
        {
            var mappedRange = new LinkedList<TTarget>();

            foreach (var source in sources)
            {
                mappedRange.AddLast(Map<TTarget>(source));
            }

            return mappedRange;
        }

        public static void InitializeMapper()
        {
            if (IsMappingInitialized)
            {
                return;
            }

            Mapper.Initialize(x => { x?.CreateMap<RequestJob, NewRequestJobDto>()?.DisableCtorValidation()?.ReverseMap(); });

            Mapper.Configuration?.CompileMappings();
            Mapper.AssertConfigurationIsValid();

            IsMappingInitialized = true;
        }
    }
}