using System;

namespace mvdata.foodjet.Filter
{
    public interface IFilterable
    {
        void AddFilter(AbstractMapFilter filter, bool autoApply = true);
        void ApplyFilters();
        void ClearFilter();
        void RemoveFilter(Type filterType);
    }
}