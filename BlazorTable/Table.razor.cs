﻿using Microsoft.AspNetCore.Components;
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
        List<TItem> filteredItems = new List<TItem>();
        public List<TItem> FilteredItems { 
            get
            { 
                if(filteredItems.Count()==0)
                {
                    return Items.ToList();
                }
                else
                {
                    return filteredItems;
                }
            }
            set 
            {
                filteredItems = value;
            } 
        } 
        public IEnumerable<TItem> DisplayedItems { get
            { 
                if(filters.Count()==0)
                {
                    return Items;
                }
                else
                {
                    return FilteredItems;
                }
            } 
        }
        public PropertyInfo[] properties;
        public List<(string, Func<TItem, string>, string)> filters = new List<(string, Func<TItem, string>, string)>();
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
        public void ApplyFilter(PropertyInfo property, string text)
        {
            Func<TItem, string> func = GetPropertyDelegate<TItem>(property);
            List<Func<TItem, bool>> predicates = new List<Func<TItem, bool>>();

            if (filters.Where(p => p.Item1 == property.Name).Count() > 0 && !string.IsNullOrEmpty(text))
            {
                var filter = filters.Where(p => p.Item1 == property.Name).First();
                filters.Remove(filter);
                filter.Item3 = text;
                filters.Add(filter);
            }
            else if(filters.Where(p => p.Item1 == property.Name).Count() > 0 && string.IsNullOrEmpty(text))
            {
                var filter = filters.Where(p => p.Item1 == property.Name).First();
                filters.Remove(filter);
            }
            else if(!(filters.Where(p => p.Item1 == property.Name).Count() > 0) && !string.IsNullOrEmpty(text))
            {
                (string, Func<TItem, string>, string) filter = (property.Name, func, text);
                filters.Add(filter);
            }
            foreach(var filter in filters)
            {
                Func<TItem, bool> predicate = p => filter.Item2(p).Contains(filter.Item3);
                predicates.Add(predicate);
            }
            foreach(var predicate in predicates)
            {
                FilteredItems = FilteredItems.Where(predicate).ToList();
            }
            Refresh();
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
        public static Func<T, string> GetPropertyDelegate<T>(PropertyInfo property)
        {
            return x => property.GetValue(x)?.ToString();
        }
    }
}
