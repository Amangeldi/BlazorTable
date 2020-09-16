using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTable
{
    public class TableBase<TItem> : ComponentBase
    {
        public const int PAGER_SIZE = 6;
        public int pagesCount;
        public int curPage;
        public int startPage;
        public int endPage;
        [Parameter]
        public IEnumerable<TItem> Items { get; set; }
        [Parameter]
        public int PageSize { get; set; }
        public IEnumerable<TItem> ItemList { get; set; }
        public List<TItem> filteredItems = new List<TItem>();
        public IEnumerable<TItem> DisplayedItems { get
            { 
                if(filteredItems.Count()==0)
                {
                    return Items;
                }
                else
                {
                    return filteredItems;
                }
            } 
        }
        public PropertyInfo[] properties;
        public Dictionary<Func<TItem, string>, string> filters = new Dictionary<Func<TItem, string>, string>();
        protected override void OnInitialized()
        {
            properties = typeof(TItem).GetProperties();
            Refresh();
        }
        public void Refresh()
        {
            curPage = 1;
            ItemList = DisplayedItems.Skip((curPage - 1) * PageSize).Take(PageSize);
            pagesCount = (int)Math.Ceiling(DisplayedItems.Count() / (decimal)PageSize);
            SetPagerSize("forward");
            StateHasChanged();
        }
        public void ApplyFilter(Func<TItem, string> func, string text)
        {
            List<Func<TItem, bool>> predicates = new List<Func<TItem, bool>>();
            // Тут нужна помощь
            if (filters.Where(p => AreMethodsEqual(p.Key.Method, func.Method)).Count() > 0 && !string.IsNullOrEmpty(text))
            {
                filters[func] = text;
            }
            else if(filters.Where(p => AreMethodsEqual(p.Key.Method, func.Method)).Count() > 0 && string.IsNullOrEmpty(text))
            {
                filters.Remove(func);
            }
            else if(!(filters.Where(p => AreMethodsEqual(p.Key.Method, func.Method)).Count() > 0) && !string.IsNullOrEmpty(text))
            {
                filters.Add(func, text);
            }
            foreach(var filter in filters)
            {
                Func<TItem, bool> predicate = p => filter.Key(p).Contains(filter.Value);
                predicates.Add(predicate);
            }
            foreach(var predicate in predicates)
            {
                filteredItems = DisplayedItems.Where(predicate).ToList();
            }
            Refresh();
        }
        public bool AreMethodsEqual(MethodBase left, MethodBase right)
        {
            MethodBody m1 = left.GetMethodBody();
            MethodBody m2 = right.GetMethodBody();
            byte[] il1 = m1.GetILAsByteArray();
            byte[] il2 = m2.GetILAsByteArray();
            return il1.SequenceEqual(il2);
        }
        public void UpdateList(int currentPage)
        {
            ItemList = DisplayedItems.Skip((currentPage - 1) * PageSize).Take(PageSize);
            curPage = currentPage;
            StateHasChanged();
        }
        public void SetPagerSize(string direction)
        {
            if (direction == "forward" && endPage < pagesCount)
            {
                startPage = endPage + 1;
                if (endPage + PAGER_SIZE < pagesCount)
                {
                    endPage = startPage + PAGER_SIZE - 1;
                }
                else
                {
                    endPage = pagesCount;
                }
                StateHasChanged();
            }
            else if (direction == "back" && startPage > 1)
            {
                endPage = startPage - 1;
                startPage = startPage - PAGER_SIZE;
            }
        }
        public void NavigateToPage(string direction)
        {
            if (direction == "next")
            {
                if (curPage < pagesCount)
                {
                    if (curPage == endPage)
                    {
                        SetPagerSize("forward");
                    }
                    curPage += 1;
                }
            }
            else if (direction == "previous")
            {
                if (curPage > 1)
                {
                    if (curPage == startPage)
                    {
                        SetPagerSize("back");
                    }
                    curPage -= 1;
                }
            }
            UpdateList(curPage);
        }
        static List<Func<T, string>> GetPropertiesDelegates<T>(PropertyInfo[] properties)
        {
            List<Func<T, string>> delegates = new List<Func<T, string>>(properties.Length);
            foreach (var pi in properties)
            {
                if (pi.CanRead)
                {
                    delegates.Add(x => pi.GetValue(x)?.ToString());
                }
            }
            return delegates;
        }
        public static Func<T, string> GetPropertyDelegate<T>(PropertyInfo property)
        {
            return x => property.GetValue(x)?.ToString();
        }
    }
}
