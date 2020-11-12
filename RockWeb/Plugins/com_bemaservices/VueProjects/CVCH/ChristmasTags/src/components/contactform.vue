<template>
    <div>
    <v-card elevation="2" class="vuecard">
        <v-app-bar absolute dark color="primary" dense scroll-target="#scrolling-techniques-7">
            <v-btn icon dark absolute right @click="closeModal">
                <v-icon small>fa-times</v-icon>
            </v-btn>
        </v-app-bar>
        <div style="height:30px;"></div>
        <v-sheet id="scrolling-techniques-7" class="overflow-y-auto pt-5" max-height="100%">
            <transition v-if="!iframeSource && !returnTagsProcessed" name="fade" appear>
                
                <div>
               
                    <v-card-text >
               
                        <v-form ref="form" v-model="valid">
                            <v-container>
                                <v-row>
                                    <v-col cols="12" sm="6">
                                        <h4>Contact Information</h4>
                                    </v-col>
                                </v-row>
                                <v-row>
                                    <v-col cols="12" sm="6">
                                        <v-text-field v-model="currentPersonFirstName" :rules="nameRules"
                                            hint="Please enter your first name" label="First name" name="fname"
                                            required></v-text-field>
                                    </v-col>
                                    <v-col cols="12" sm="6">
                                        <v-text-field v-model="currentPersonLastName" :rules="nameRules"
                                            hint="Please enter your last name" label="Last Name" name="lname" required>
                                        </v-text-field>
                                    </v-col>
                                </v-row>
                                <v-row>
                                    <v-col cols="12" sm="6">
                                        <v-text-field v-model="currentPersonEmail" :rules="emailRules"
                                            hint="Please enter your email" label="Email" name="email" required>
                                        </v-text-field>
                                    </v-col>
                                </v-row>
                                <v-row>
                                    <v-col cols="12" sm="12">
                                        <h4>Confirm Your Tags</h4>

                                        <v-simple-table fixed-header>
                                            <template v-slot:default>
                                                <thead>
                                                    <tr>
                                                        <th class="text-left">
                                                            Age Range
                                                        </th>
                                                        <th class="text-left">
                                                            Gender
                                                        </th>
                                                        <th class="text-left">
                                                            Description
                                                        </th>
                                                        <th>Quantity</th>
                                                        <th>Suggested Donation</th>
                                                        <th class="text-center">Delete</th>
                                                    </tr>
                                                </thead>


                                                <tbody>
                                                    <tagTableRow 
                                                        v-for="tag in pulledTags" 
                                                        :key="tag.id" 
                                                        :pulledTag="tag" 
                                                        :fulfillment="fulfillment" 
                                                        v-on:remove-tag="removeTags"
                                                    />
                                                    </tbody>
                                                    <tfoot>
                                                <tr>
                                                    <th></th>
                                                    <th></th>
                                                    
                                                    <th colspan="2" class="text-right">Suggested Financial Donation:</th>
                                                    <th>{{financialDonation}}</th>
                                                </tr>
                                                    </tfoot>
                                                  
                                            </template>
                                        </v-simple-table>
                                    </v-col>
                                </v-row>

                                <v-row class="d-flex justify-end">
                                    <v-col cols="12" sm="4" class="d-flex flex-column justify-start">


                                        <v-radio-group v-model="fulfillment" column
                                            label="How would you like to fulfill these tags?" :rules="fulfillmentRules">

                                            <v-radio label="Monetary Donation" value="donation"></v-radio>
                                            <v-radio label="Buy Gifts" value="buygifts" v-if=" !everyFinancialTransactionCheck"></v-radio>
                                        </v-radio-group>

                                    </v-col>
                                    <v-col cols="12" sm="8" style="min-height:200px;"
                                        class="d-flex flex-column justify-start mt-5">
                                        <transition name="slideleft" mode="out-in">
                                            <v-alert v-if="fulfillment == 'donation'" border="top" colored-border
                                                type="primary" elevation="2" icon="fa-money-bill-alt">
                                                By selecting "Monetary Donation", you will be redirected to our donation
                                                page with a suggested donation of {{financialDonation}}. Please note that designated funds can be redirected by the Executive Leadership Team to other ministry needs. This will only be done if absolutely necessary.
                                            </v-alert>
                                        </transition>
                                        <transition name="slideleft" mode="out-in">
                                            <v-alert v-if="fulfillment == 'buygifts'" border="top" colored-border
                                                type="accent" elevation="2" icon="fa-shopping-cart">
                                                By selecting "Buy Gifts", you agree to pruchase gifts for the christmas
                                                store for each tag you select and return them to the church during a
                                                designated drop off time.
                                                <span v-if="financialTransactionCheck && !everyFinancialTransactionCheck"><br /><br />
                                                    You have tags that require a financial donation, you will be asked to make a suggested donation of {{financialDonation}}. You will receive an email with instructions for donating items.
                            
                                                </span> 
                                            </v-alert>
                                        </transition>

                                    </v-col>
                                </v-row>
                            </v-container>
                            <v-card-actions>
                                <v-container>
                                    <v-row>
                                        <v-col cols="12" sm="12">
                                            <div class="float-right">
                                                <v-btn color="primary" class=" mr-4"
                                                    :class="fulfillment =='donation' ? 'primary' : 'accent'"
                                                    :disabled="!formvalid" @click="submit">
                                                    {{buttonText}}
                                                </v-btn>
                                                <v-btn color="warning" @click="cancelButton">
                                                    Cancel<sup>*</sup>
                                                </v-btn>

                                            </div>
                                            <br><br><span class="pull-right warning--text mt-1"
                                                style="font-size:.8rem;"><sup>*</sup> Clicking cancel will return all of
                                                your pulled tags.</span>
                                        </v-col>
                                    </v-row>
                                </v-container>
                            </v-card-actions>
                        </v-form>
                    </v-card-text>
                </div>
            </transition>
            <transition v-else-if="iframeSource" name="fade" mode="out-in">
                <iFrame :src="iframeSource" />
            </transition>
            <transition name="fade" v-else mode="out-in">
                <transactionComplete v-on:closeModal="closeModal()"/>
            </transition>
        </v-sheet>
    </v-card>
</div>
</template>
<script>
import { EventBus } from '../modules/event-bus.js';
const iFrame = () => import(
    /* webpackChunkName: "iFrame" */ './iFrame'
  );
const transactionComplete = () => import(
/* webpackChunkName: "transactionComplete" */ './transactionComplete'
);
const tagTableRow = () => import(
  /* webpackChunkName: "transactionComplete" */ './tagTableRow'  
)



export default {

    components:{
        iFrame,
        transactionComplete,
        tagTableRow
    },

    mounted(){
        EventBus.$on('transactionComplete', (data) => {
           setTimeout(() => {
              this.setTransactionValue(data);
           },2000)
           
        });
    },

    data: () => ({
        valid:false,
        baseDonation:25,
        iframeSource:null,
        tagResponse:null,
        showSuccess:false,
        fulfillment:null,
        
        nameRules: [
        v => !!v || 'Name is required',
        v => !!v && v.length <= 35 || 'Name must be less than 35 characters',
      ],
         emailRules: [
        v => !!v || 'E-mail is required',
        v => /.+@.+/.test(v) || 'E-mail must be valid',
      ],
        fulfillmentRules: [
            v => !!v || 'A Fulfillment method is required.'
        ],
    }),
    methods:{

       
        
        closeModal(){
            this.iframeSource = null;
            this.transactionInfo = null;
            this.showSuccess = false;
            this.$store.commit('updateTagsProcessed',false);
            this.$emit('close-modal');
        },
        removeTags(id){
           this.$store.commit('removeTag',id);
        },
        removeAllTags(id){
           this.$store.commit('removeAllTags');
        },
        cancelButton(){
            this.removeAllTags()
            this.iframeSource = null;
            this.closeModal()
        },
        async submit(){        
            // check if current person is blank

            if(this.fulfillment == 'donation' || this.financialTransactionCheck) {
                let args = '?AccountIds='
                this.calculatedAccountValues.forEach(a => {
                    args += `${a.accountId}^${a.quantity * a.suggestedDonation}^true,`
                })
                let url = 'https://my.covechurch.org/donatetags2' + args
                this.iframeSource = url
            } else {
                this.$store.dispatch('processTags')
            }
        },
    },

    watch:{

        fulfillment: function(val){
            this.$store.commit('updatePulledTagFulfillment',val)
        },
        returnedTransactionInfo: function(val){
            if(val){
                this.iframeSource = null;
            }
        }
    },

    computed:{
        returnedTransactionInfo(){
          return this.$store.state.financialData.TransactionId;
       },

     returnTagsProcessed(){
         return this.$store.state.tagsProcessed;
     },
        returnedTransactionInfo(){
          return this.$store.state.financialData;
       },
      calculatedAccountValues(){
            return this.$store.getters.getAccountTotals;
        },
      financialDonation(){
          return '$ ' + this.calculatedAccountValues.reduce((acc,val) => 
            {
                return val.quantity * val.suggestedDonation
            },0).toFixed(2)
      },
       currentPersonFirstName: {
           get(){
            return this.$store.state.currentPerson.firstName;
           },
            set (value) {
                this.$store.commit('setCurrentPersonFirstName', value)
            }
       },
        currentPersonLastName: {
           get(){
            return this.$store.state.currentPerson.lastName;
           },
            set (value) {
                this.$store.commit('setCurrentPersonLastName', value)
            }
       },
       currentPersonEmail: {
           get(){
            return this.$store.state.currentPerson.email;
           },
            set (value) {
                this.$store.commit('setCurrentPersonEmail', value)
            }
       },
        
        pulledTags() {
            return this.$store.state.pulledTags;
        },
        financialTransactionCheck() {
            // Checks if some of the items
            return this.pulledTags.some(e => {
                return e.requireFinancialDonation == true
            })
        },
        everyFinancialTransactionCheck() {
            // Returns false if every tag is not a require financial donation.  This checks if there is a mixture of tags to adjust the ui accordingly.
            return this.pulledTags.every(e => {
                return e.requireFinancialDonation == true
            })
        },
        formvalid(){
            //Uses the form rules array and makes sure that there are tags pulled. This is used to disable/enable the submit button.
            if(this.valid && this.pulledTags.length > 0){
                return true
            } else {
                return false
            }
        },
        buttonText(){
            if(this.fulfillment == 'donation') {
                return 'Make Donation'
            } else {
                return 'Claim Tags'
            }
        }
    }
}
</script>

<style scoped>
tfoot th {
    font-size:1.0rem !important; 
}
.vuecard {
    height: 95vh;
    width: 95vw;
    overflow-y:scroll;
    
}

.slideleft-leave-active,
.slideleft-enter-active {
  transition: .5s linear;
  position:absolute;
}
.slideleft-enter-active {
  position:absolute;
  z-index:20;
}

.slideleft-enter {
    opacity:0;
    position:absolute;
  transform: translate(100%, 0);
}
.slideleft-leave-to {
  
  transform: translate(100%, 0);
}
.slideleft-enter-to {
    opacity:1;
}

.fade-enter-active, .fade-leave-active {
  transition: opacity .5s;
}
.fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
  opacity: 0;
}
</style>