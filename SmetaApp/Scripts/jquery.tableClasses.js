;(function($){
	"use strict";
	//<summary>Add classes to the last tr</summary>
	//<return>this</return>
	$.fn.extend({
	    trAddCl: function(...classes) {
	    	for(let className of classes)
	    		this.find("tr").last().addClass(className);
			return this;
	    }
	});
	$.fn.extend({
	    tdsAddDType: function(...classes) {
	    	let tds=this.find("tr").last().find("td");
	    	for(let i=0,k=0;i<classes.length&&k<tds.length;i++,k++){
	    		if(typeof classes[i] =="number"){
	    			k+=classes[i]-1;
	    			continue;
                }
                let td = tds.eq(k);
                td.data("type", classes[i]);
                td.attr('data-type', classes[i]); 
    	    }
			return this;
	    }
    });
    $.fn.extend({
        trAddClAt: function (...classes) {
            for (let className of classes)
                this.addClass(className);
            return this;
        }
    });
    $.fn.extend({
        tdsAddDTypeAt: function (...classes) {
            let tds = this.find("td");
            for (let i = 0, k = 0; i < classes.length && k < tds.length; i++ , k++) {
                if (typeof classes[i] == "number") {
                    k += classes[i] - 1;
                    continue;
                }
                let td = tds.eq(k);
                td.data("type", classes[i]);
                td.attr('data-type', classes[i]);
            }
            return this;
        }
    });
})(jQuery);