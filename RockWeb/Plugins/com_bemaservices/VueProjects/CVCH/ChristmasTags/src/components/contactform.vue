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
            <transition v-if="!iframeSource && !showSuccess" name="fade" appear>
                
                <div>
               
                    <v-card-text v-show="!transactionInfo">
               
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
                                                        <th class="text-center">Delete</th>
                                                    </tr>
                                                </thead>


                                                <tr>

                                                </tr>


                                                <tbody>
                                                    <tr v-for="tag in pulledTags" :key="tag.id">
                                                        <td>{{tag.ageRange.description}}</td>
                                                        <td>{{tag.gender.description}}</td>
                                                        <td>{{tag.description}} <span v-if="tag.requireFinancialDonation" class="warning--text"><br />This tag is fulfilled by a financial donation.</span></td>
                                                        <td class="text-center">
                                                            <v-icon @click="removeTags(tag.id)" color="secondary" small>
                                                                fa-trash</v-icon>
                                                        </td>

                                                    </tr>
                                                </tbody>
                                            </template>
                                        </v-simple-table>
                                    </v-col>
                                </v-row>

                                <v-row class="d-flex justify-end">
                                    <v-col cols="12" sm="4" class="d-flex flex-column justify-start">


                                        <v-radio-group v-model="fulfillment" column
                                            label="How would you like to fulfill these tags?" :rules="fulfillmentRules">

                                            <v-radio label="Monetary Donation" value="donation"></v-radio>
                                            <v-radio v-if="!financialTransactionCheck" label="Buy Gifts" value="buygifts"></v-radio>
                                        </v-radio-group>

                                    </v-col>
                                    <v-col cols="12" sm="8" style="min-height:200px;"
                                        class="d-flex flex-column justify-start mt-5">
                                        <transition name="slideleft" mode="out-in">
                                            <v-alert v-if="fulfillment == 'donation'" border="top" colored-border
                                                type="primary" elevation="2" icon="fa-money-bill-alt">
                                                By selecting "Monetary Donation", you will be redirected to our donation
                                                page with a suggested donation of $25.00 per tag for a total gift of
                                                ${{formatToCurrency(tagDonation)}}. Please note that designated funds can be redirected by the Executive Leadership Team to other ministry needs. This will only be done if absolutely necessary.
                                                <span v-if="financialTransactionCheck && !mixedTagCheck"><br /><br />
                                                Your pulled tags list included tags that are fulfilled by a financial donation and tags that can be fulfilled by buying gifts.  If you wish to buy and donate gifts, you will need to remove the gift donation tags complete the process separetely for those tags.
                                                       
                                                </span> 
                                            </v-alert>
                                        </transition>
                                        <transition name="slideleft" mode="out-in">
                                            <v-alert v-if="fulfillment == 'buygifts'" border="top" colored-border
                                                type="accent" elevation="2" icon="fa-shopping-cart">
                                                By selecting "Buy Gifts", you agree to pruchase gifts for the christmas
                                                store for each tag you select and return them to the church during a
                                                designated drop off time.
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
                <transactionComplete  :transactionInfo="transactionInfo" :fulfillmentType="fulfillment" :responseMessage="responseMessage" v-on:closeModal="closeModal()"/>
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



export default {

    components:{
        iFrame,
        transactionComplete
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
        transactionInfo:null,
        tagResponse:null,
        showSuccess:false,
        fulfillment:null,
    
        nameRules: [
        v => !!v || 'Name is required',
        v => v.length <= 35 || 'Name must be less than 35 characters',
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
        processTags(){
            
            let pulledTagIds = this.$store.state.pulledTagIds;
            let url = `/Webhooks/Lava.ashx/BEMA/ProcessChristmasTags/${this.currentPerson.firstName}/${this.currentPerson.lastName}/${this.currentPerson.email}/${pulledTagIds.join(',')}/${this.transactionInfo ? this.transactionInfo.PrimaryPerson : 0}/${this.transactionInfo ? this.transactionInfo.TransactionGuid: 0}`
            fetch(url)
                .then(response => response.json())
                .then(data => {
                    console.log(data)
                    this.responseMessage = data.SuccessText;
                    this.tagResponse = data;
                    this.showSuccess = true;
                    
                })
                .catch(er => console.log(er));
            
            EventBus.$emit('TagsPulled',pulledTagIds);
                    
        },
        setTransactionValue(data){
            this.transactionInfo = data;
            this.responseMessage = data.SuccessText;
            this.iframeSource = null;
            this.showSuccess = true;
            this.processTags();
        },
        closeModal(){
            console.log('showModal from modal')
            this.iframeSource = null;
            this.transactionInfo = null;
            this.showSuccess = false;
            this.$emit('close-modal')
        },
        removeTags(id){
            if(id){
                EventBus.$emit('deleteItem',id);
            } else {
                EventBus.$emit('deleteAllTags')
            }
        },
        cancelButton(){
            this.removeTags()
            this.iframeSource = null;
            this.closeModal()
        },
        submit(){
            
            if(this.fulfillment == 'donation') {
                let args = `?AccountIds=166^${this.tagDonation}^true`
                let url = 'https://my.covechurch.org/page/932' + args
                this.iframeSource = url
            } else {
                this.processTags();
            }
            
        },
        formatToCurrency(amount){
           return (amount).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,'); 
        }


    },
    watch:{
        tags: function(){
          if( this.pulledTags.some(e => {
                return e.requireFinancialDonation === true
            })) {
                this.fulfillment = 'donation';
            }
        },
 
    },
    computed:{
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

            return this.pulledTags.some(e => {
                return e.requireFinancialDonation == true
            })


        },
        mixedTagCheck() {

            return this.tags.every(e => {
                return e.requireFinancialDonation == true
            })


        },
        formvalid(){
            if(this.valid && this.pulledTags.length > 0){
                return true
            } else {
                return false
            }
        },
        tagDonation(){
            return this.pulledTags.length * 25;
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