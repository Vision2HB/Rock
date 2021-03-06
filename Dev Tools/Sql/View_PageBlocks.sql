SELECT 
	s.Name [Site.Name]
	,p.InternalName [Page.Title]
    ,p.[Guid] [Page.Guid]
    ,b.NAME [Block.Name]
    ,b.[Guid] [Block.Guid]
	,b.CreatedDateTime
    ,bt.NAME [BlockType.Name]
FROM [Block] [b]
JOIN [Page] [p] ON [p].[Id] = [b].[PageId]
JOIN [BlockType] [bt] ON [bt].[Id] = [b].[BlockTypeId]
JOIN [Layout] [l] ON p.LayoutId = l.Id
JOIN [Site] [s] ON l.SiteId = s.Id
ORDER BY s.Name desc, p.InternalName
    ,b.NAME
