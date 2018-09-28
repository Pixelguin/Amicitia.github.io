$(document).ready(function(){
	jQuery(".toggle .toggle-title").click(function() {
	  if (jQuery(this).hasClass("active")) {
		jQuery(this)
		  .removeClass("active")
		  .closest(".toggle")
		  .find(".toggle-inner")
		  .slideUp(200);
	  } else {
		jQuery(this)
		  .addClass("active")
		  .closest(".toggle")
		  .find(".toggle-inner")
		  .slideDown(200);
	  }
	});
});

function copyDivToClipboard(id) {
	var range = document.getSelection().getRangeAt(0);
	range.selectNode(document.getElementById(id));
	window.getSelection().addRange(range);
	document.execCommand("copy");
	$('.copied').show();
	$('.copied').fadeOut(1500);
}



