The Dynamic Report With Print block and the Report Detail With Lava block enable putting a bare report grid or a lava formatted set of grid data on a blank page so that it can be printed to PDF in a reasonable format.

The Report Detail With Lava block can be used interchangeably with the Report Detail block, or used in addition to it.  The Dynamic Report With Print block replaces the default Dynamic Data block to enable passing the necessary parameters to the Report Detail With Lave block for printing.  Here are the intended uses. 

1) Create a PDF printable Report Grid.

	a) Create a new page (I usually do this under the current Tools->Reporting->Reports page).
	b) Place a Report Detail With Lava block on the page.
	c) Set the Layout of the page to Blank (this removes the menus, etc., for clean printing).
	d) On the Block Properties, set the Use Printable Format to Yes.  This removes grid paging and other widgets from the blocks.

	Option 1 - the Geek version I like better - opens the window in a new tab.

		d) Make a note of the page number created above.
		e) Add the following code to the existing Reports page Report Detail block in the Advanced Settings Pre-HTML field:

<script>
   $(function(){
     $("<a title='Printable PDF' class='btn-add btn btn-default btn-sm' href='/page/[PAGE NUMBER]?ReportId={{ 'Global' | PageParameter:'ReportId' }}' target='_blank'><i class='fa fa-print'></i></a>").insertAfter(".grid-actions a.btn:last-of-type")
   })
</script>

		f) Replace [PAGE NUMBER] in the above code with the actual page number created.

	Option 2 - doesn't require javascript but will open in the same window.

		d) On the existing Reports page, replace the Report Detail block with the Report Detail With Lava block.
		e) On the block settings, set the Data View Page to point to Data Views page and set the Print Action Page to the page you created in step 1-a.

Both Option 1 and 2 add the print icon to the screen and navigate to the new page with the ReportId in the url.



2) Using Lava Output

	a) Follow the steps above to create the page and enable the icon navigation.
	b) On the new page's Report Detail With Lava block, use lava in the Formatted Output field of the block settings to format the output.   This will override the default grid format.  Note that all fields on the report should be available by looping through {% for row in Rows %}.  Field names with spaces removed are the lava keys such as {{ row.FirstName }}.  This should operate the same as other Formatted Output lava scripts in Rock.

One caveat - you may need to create a separate page with lava for each report since the lava is specific to the fields on the report.  While one page can print the grids of any report, lava is specific to the fields on the report, so more pages may be needed.


3) Dynamic Reports

Basically, the Dynamic Report With Print block adds a feature that enables calling the page created in Part 1 with the ReportId and also a Json text attribute that contains the Data View Filter Overrides set on the Dynamic Report page.  So the same page can be used to print Reports or Dynamic Reports with parameters in the URL.

	a) Follow the steps in 1) a through d to create the new page with a Report Detail With Lava block.
	b) Create a page with the Dynamic Reporting With Print block.
	c) Set the Report and the filter options on the Custom Page Attributes of the Dynamic Report With Print block.
	d) On the block properties, set the Print Action Page attribute to point to the new page containing the Report Detail With Lava block.

The print icon will show once the Print Action Page attribute is set and will allow navigation to the new page with filter parameters set.

Enjoy.
