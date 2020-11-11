<template>
<v-container class="pt-12">
    <v-img
        contain
        max-height="250"
        src="../Assets/images/CWTC_Title_English.jpg"
      ></v-img>
            <v-alert
                border="top"
                colored-border
                >
                <p>Thank you for your donation.  Please watch your email for any additional information or instructions.</p>
                <div v-if="returnedTransactionInfo.TransactionId">
                    
                    <p>Transaction Details:</p>
                    <ul>
                        <li><strong>Total Amount: </strong>${{returnedTransactionInfo.TotalAmount}}</li>
                        <li><strong>Payment Type: </strong>{{returnedTransactionInfo.PaymentType}}</li>
                        <li><strong>Transaction Code: </strong>{{returnedTransactionInfo.TransactionCode}}</li>
                        <li><strong>Transaction Code: </strong>{{returnedTransactionInfo.TransactionDateTime}}</li>
                    </ul>
                </div>

                <v-btn :class="fulfillmentType =='donation' ? 'success' : 'info'" @click="closeModal">
                    Close Window
                </v-btn>
            </v-alert>
        
                
        
</v-container>
</template>
<script>

export default {
    name: 'transactionComplete',
    // props:{
    //     transactionInfo: Object,
    //     tagResponse: Object,
    //     fulfillmentType: String,
    //     responseMessage: String
    // },
    methods:{
        closeModal(){
            this.$store.commit('updateTagsProcessed',false)
            this.$store.dispatch('updateTagsProcessed',false)
            this.$emit('closeModal')
            
        }
    },
    computed:{
        returnedTransactionInfo(){
          return this.$store.state.financialData;
       },
    }
}
</script>
<style scoped>

</style>