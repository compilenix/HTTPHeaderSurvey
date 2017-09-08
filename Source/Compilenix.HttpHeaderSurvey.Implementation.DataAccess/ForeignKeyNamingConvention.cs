using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

// ReSharper disable All

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess
{
    public class ForeignKeyNamingConvention : IStoreModelConvention<AssociationType>
    {
        private static bool DoPropertiesHaveDefaultNames(IReadOnlyCollection<EdmProperty> properties, IReadOnlyList<EdmProperty> otherEndProperties)
        {
            if (properties.Count != otherEndProperties.Count)
            {
                return false;
            }

            return !properties.Where((property, i) => !property?.Name?.EndsWith("_" + otherEndProperties[i]?.Name) ?? false).Any();
        }

        private static void NormalizeForeignKeyProperties(IReadOnlyCollection<EdmProperty> properties)
        {
            if (properties == null)
            {
                return;
            }

            foreach (var t in properties)
            {
                var defaultPropertyName = t?.Name;
                if (defaultPropertyName == null)
                {
                    continue;
                }

                var ichUnderscore = defaultPropertyName.IndexOf('_');

                if (ichUnderscore <= 0)
                {
                    continue;
                }

                var navigationPropertyName = defaultPropertyName.Substring(0, ichUnderscore);
                var targetKey = defaultPropertyName.Substring(ichUnderscore + 1);

                string newPropertyName;
                if (targetKey.StartsWith(navigationPropertyName))
                {
                    newPropertyName = targetKey;
                }
                else
                {
                    newPropertyName = navigationPropertyName + targetKey;
                }
                t.Name = newPropertyName;
            }
        }

        public void Apply(AssociationType association, DbModel model)
        {
            if (!association.IsForeignKey)
            {
                return;
            }

            var constraint = association.Constraint;
            if (DoPropertiesHaveDefaultNames(constraint?.FromProperties, constraint?.ToProperties))
            {
                NormalizeForeignKeyProperties(constraint?.FromProperties);
            }
            if (DoPropertiesHaveDefaultNames(constraint?.ToProperties, constraint?.FromProperties))
            {
                NormalizeForeignKeyProperties(constraint?.ToProperties);
            }
        }
    }
}