
    /** Watch all check boxes in the batches class for when they are checked.*/
    function watchChecks() {
        $('.batches input[type=checkbox]').change(function(){
            var el = $(this);
            var batchId = el.data("batchid");

            if(el.is(':checked')) {
                document.getElementById(batchId).classList.remove("hidden-print");
                
            } else {
                document.getElementById(batchId).classList.add("hidden-print")
            }
        })
    }

    function confirmDepositWatch() {
        $('.batches a.confirmdeposit').click(function(){
            let el = $(this);

            let transactioncodes = el.data("transactionlist");
            let batchid = el.data("batchid");
            
            
            finalizedeposit(transactioncodes, batchid)
        
        })
    }

/** Api Calls to Update(Reconcile) tranasactions */

async function finalizedeposit(transactioncodes, batchid) {
    
    let today = new Date().toLocaleDateString("en-US")

    let codeTransformed = transactioncodes.split(',')

    await Promise.all(codeTransformed.map((x) => {

         setIsReconciled(x);
         setReconciledDate(x,today);

    }))

    $('#confirmbatch-' + batchid).addClass("hide");
    $('#printbatch-' + batchid).removeClass("hide");

    codeTransformed.map((y) => { addconfirmed(y,today) })

}

function setIsReconciled(item) {
    let isReconciledUrl = `/api/FinancialTransactions/${item}`

    fetch(isReconciledUrl, {
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json'
        },
        method: 'PATCH',                                                              
        body: JSON.stringify( { "IsReconciled": "true" } )                                        
      })
}

function setReconciledDate(item,today){
    
    let reconcileurldate = `/api/FinancialTransactions/AttributeValue/${item}?attributeKey=ReconciledOn&attributeValue=${today}`

    fetch(reconcileurldate,{
        method:'POST',
        credentials:'include'
    })

}


function addconfirmed(item,today) {
    
    document.getElementById(item).innerHTML = `${today}`
}

$( document ).ready( function() {
    watchChecks();
    confirmDepositWatch();
});
var prm = Sys.WebForms.PageRequestManager.getInstance();

prm.add_endRequest( function() {
    watchChecks();
    confirmDepositWatch();
});