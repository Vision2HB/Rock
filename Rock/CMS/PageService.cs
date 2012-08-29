//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;

using Rock.Data;

namespace Rock.Cms
{
	/// <summary>
	/// Page Service class
	/// </summary>
	public partial class PageService : Service<Page, PageDto>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PageService"/> class
		/// </summary>
		public PageService() : base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PageService"/> class
		/// </summary>
		public PageService(IRepository<Page> repository) : base(repository)
		{
		}

		/// <summary>
		/// Creates a new model
		/// </summary>
		public override Page CreateNew()
		{
			return new Page();
		}

		/// <summary>
		/// Query DTO objects
		/// </summary>
		/// <returns>A queryable list of DTO objects</returns>
		public override IQueryable<PageDto> QueryableDto()
		{
			return this.Queryable().Select( m => new PageDto()
				{
					Name = m.Name,
					Title = m.Title,
					IsSystem = m.IsSystem,
					ParentPageId = m.ParentPageId,
					SiteId = m.SiteId,
					Layout = m.Layout,
					RequiresEncryption = m.RequiresEncryption,
					EnableViewState = m.EnableViewState,
					MenuDisplayDescription = m.MenuDisplayDescription,
					MenuDisplayIcon = m.MenuDisplayIcon,
					MenuDisplayChildPages = m.MenuDisplayChildPages,
					DisplayInNavWhen = m.DisplayInNavWhen,
					Order = m.Order,
					OutputCacheDuration = m.OutputCacheDuration,
					Description = m.Description,
					IncludeAdminFooter = m.IncludeAdminFooter,
					CreatedDateTime = m.CreatedDateTime,
					ModifiedDateTime = m.ModifiedDateTime,
					CreatedByPersonId = m.CreatedByPersonId,
					ModifiedByPersonId = m.ModifiedByPersonId,
					IconUrl = m.IconUrl,
					Id = m.Id,
					Guid = m.Guid,				});
		}
	}
}
