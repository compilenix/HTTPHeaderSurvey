using System.Collections.Generic;
using AutoMapper;
using Integration.DataAccess.Entitys;
using Integration.Domain.DataTransferObjects;

namespace Implementation.Domain
{
    public static class MappingUtils
    {
        private static bool IsMappingInitialized { get; set; }

        public static TTarget Map<TTarget>(object source)
        {
            return Mapper.Map<TTarget>(source);
        }

        public static TTarget Map<TTarget>(object source, object target) where TTarget : class
        {
            return Mapper.Map(source, target) as TTarget;
        }

        public static IEnumerable<TTarget> MapRange<TTarget>(IEnumerable<object> sources)
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

            Mapper.Initialize(x =>
            {
                x?.CreateMap<RequestJob, NewRequestJobDto>()?.DisableCtorValidation()?.ReverseMap();
            });

            Mapper.Configuration?.CompileMappings();
            Mapper.AssertConfigurationIsValid();

            IsMappingInitialized = true;
        }
    }
}