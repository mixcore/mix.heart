﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.MixEntities;
using Mix.Heart.UnitOfWork;
using Mix.Heart.ViewModel;

namespace Mix.Example.Application.ViewModel
{
    public class ProductViewModel : ViewModelBase<Guid, ProductEntity, MixDbContext>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Producer { get; set; }

        /// <summary>
        /// TODO: consider with JsonIgnore attribute
        /// </summary>
        [JsonIgnore]
        public Guid CategoryId { get; set; }

        public List<ProductDetailViewModel> ProductDetails { get; set; }

        protected override void SaveEntityRelationship(ProductEntity parentEntity, UnitOfWorkInfo uowInfo)
        {
            // TODO: Save view list need to improve
            foreach (var detail in ProductDetails)
            {
                detail.ProductId = parentEntity.Id;
                detail.Save(false, uowInfo);
            }
        }
    }
}
