﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Zing.Framework.Mvc;
using Zing.Framework.Utility;

namespace Zing.Framework.UI.Grid.Fluent
{
    public class GridColumnFactory<TModel> : IHideObjectMembers where TModel : class
    {
        public Grid<TModel> Container
        {
            get;
            private set;
        }
        public GridColumnFactory(Grid<TModel> container)
        {
            Guard.IsNotNull(container, "container");

            Container = container;
        }

        public virtual GridBoundColumnBuilder<TModel> Bound<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            Guard.IsNotNull(expression, "expression");

            GridBoundColumn<TModel, TValue> column = new GridBoundColumn<TModel, TValue>(Container, expression);

            Container.Columns.Add(column);

            return new GridBoundColumnBuilder<TModel>(column);
        }
    }
}