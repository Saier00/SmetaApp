;(function($){
	"use strict";
	//<summary>Add row of headers in table</summary>
	//<return>this</return>
	$.fn.extend({
	    addThs: function(...args) {
	    	let tr=$("<tr></tr>");
	    	let cs=0;
	    	for(let arg of args){
	    		if(typeof arg =="number"){
					cs=arg;
					continue;
	    		}
	    		if(cs==0){
	    			tr.append("<th>"+arg+"</th>");
	    		}
	    		else{
					tr.append("<th colspan=\""+cs+"\">"+arg+"</th>");
					cs=0;
	    		}
			}
			this.append(tr);
			return this;
	    }
	});
	//<summary>Add row of td elements in table</summary>
	//<return>this</return>
	$.fn.extend({
	    addTds: function(...args) {
	    	let ths=this.find("tr").first().find("th");
	    	let i=0;
	    	ths.filter("[colspan]").each(function(ind,el){
	    		i+=el.colSpan;
	    	});
	    	let thsLength=ths.not("[colspan]").length+i;
	    	let tr=$("<tr></tr>");
	    	for(let arg of args){
				if(typeof arg =="number")
					for(let k=0; k<arg;k++)
						tr.append("<td></td>");
				else
					tr.append("<td>"+arg+"</td>");
			}
			for(let i=tr.find("td").length;i<thsLength;i++)
				tr.append("<td></td>");
			this.append(tr);
			return this;
	    }
    });
    $.fn.extend({
        addTdsAt: function (...args) {
            let ths = this.closest("table").find("tr").first().find("th");
            let i = 0;
            ths.filter("[colspan]").each(function (ind, el) {
                i += el.colSpan;
            });
            let thsLength = ths.not("[colspan]").length + i;
            let tr = $("<tr></tr>");
            for (let arg of args) {
                if (typeof arg == "number")
                    for (let k = 0; k < arg; k++)
                        tr.append("<td></td>");
                else
                    tr.append("<td>" + arg + "</td>");
            }
            for (let i = tr.find("td").length; i < thsLength; i++)
                tr.append("<td></td>");
            this.after(tr);
            return this;
        }
    });
})(jQuery);