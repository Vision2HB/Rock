import Vue from 'vue'
import Vuex from 'vuex'
import listenforIframeMessage from './plugins/listenforIframeMessage'
// import intersectionObserver from './plugins/intersectionObserver'

Vue.use(Vuex)

//default production endpoints
let tagListUrl = '/Webhooks/Lava.ashx/BEMA/GetChristmasTags';
let ageRangesUrl = '/Webhooks/Lava.ashx/BEMA/GetAgeRanges';
let processTagsUrl = '/Webhooks/Lava.ashx/BEMA/ProcessChristmasTags1';
let getCurrentPersonUrl = '/api/People/GetCurrentPerson';

//if in development, replace the deefault endponts with sample json objects
if(process.env.NODE_ENV == 'development') {
   tagListUrl = '/backend/tagsList.json';
   ageRangesUrl = '/backend/ageRanges.json';
   getCurrentPersonUrl = '/backend/currentPerson.json';
   processTagsUrl = '/backend/tagProcessResponse.json'
}

export default new Vuex.Store({
  state: {
    // From App.Vue
    selectedGenders:[], // the options selected in the dropdown.
    selectedAgeRanges:[],
    selectedCampus: 1,
    pulledTags:[],
    colorOptions:[{
      id: 1,
      scale: 1,
      rules: ['rule-red', 'rule-shape'],
      shape: '<i class="fas fa-sleigh" style="transform:translateY(-10px);"></i>', //&starf;
    },
    {
      id: 2,
      scale: 1,
      rules: ['rule-diagonal'],
      shape: '',
    },
    {
      id: 3,
      scale: .9,
      rules: ['rule-shape'],
      shape: '<i class="fas fa-gifts" style="transform:translateY(-15px);"></i>', //&#10052;
    },
  ],
    campusOptions:[
      { "id":1, "description":"Mooresville Campus" }, { "id":31, "description":"Denver Campus" }, { "id":35, "description":"Statesville Campus" }, { "id":36, "description":"West Rowan Campus" }
    ],
    genderOptions: [{
            description:'Boy',
            id:1
          },{
            description:'Girl',
            id:2
          }],
    ageRangeOptions: [],
    // From tagList.Vue
    tagList: [],
    start: 0,
    stepSize: 10,
    step:2,
    currentPerson:{
      firstName:null,
      lastName:null,
      email:null,
      id:null,
    },
    tagsProcessed:false,
    gettingTags:false,
    financialData:{
      TransactionCode:null,
      TransactionDateTime:null,
      PrimaryPerson:null,
      TotalAmount:null,
      PaymentType: null,
      SuccessText:null,
      TransactionId:null
    }
  },
  getters:{
    currentTagIds(state) {
      return state.tagList.map(tag => tag.id)
    },
    getTag: (state) => (id) => {return state.tagList.find(tag => tag.id == id)},
  
    getAccountTotals(state){

      var result = [];
      state.pulledTags.filter(e => e.requireFinancialDonation == true || e.fulfillment == 'donation')
      .reduce(function (res, value) {
        
          if (!res[value.accountId]) {
              res[value.accountId] = {
                  quantity: 0,
                  suggestedDonation: value.suggestedDonation,
                  accountId: value.accountId
              };
              result.push(res[value.accountId])
          }
          res[value.accountId].quantity += value.quantity
          return res;
        
      }, {});
    
      return result

    },
    filterTags(state){
      let filteredList = state.tagList;
      let pulledIds = state.pulledTags.map(tag => tag.id);  

      if(filteredList.length > 0) {
        if(pulledIds && pulledIds.length > 0 ) {
          filteredList = filteredList.filter(tag => pulledIds.includes(tag.id) == false)
        }

        if(state.selectedGenders && state.selectedGenders.length > 0){
          filteredList = filteredList.filter(tag => state.selectedGenders.includes(tag.gender.id) == true)
        }

        if(state.selectedAgeRanges && state.selectedAgeRanges.length > 0){
          filteredList = filteredList.filter(tag => state.selectedAgeRanges.includes(tag.ageRange.id) == true)
        }
        if(state.selectedCampus && state.selectedCampus> 0){
          filteredList = filteredList.filter(tag => state.selectedCampus == tag.campusId)
        }
        return filteredList.slice(0,state.step * state.stepSize);
    }
    },
    getCurrentSteps(state){
      return {currentStep: state.step, stepSize: state.stepSize}
    },
 

  },
  mutations: {
    // Initiated by the get tags action to concat the results into the tagsList store item.
    updateTags(state, tags) {
        state.tagList = state.tagList.concat(tags);
        state.step ++ ;
    },
    // Remove individual tag from pulled tag list.
    removeTag(state,tag) {
        let indexOf = state.pulledTags.findIndex(item => item.id === tag);
        state.pulledTags.splice(indexOf,1);
    },
    removeAllTags(state){
      state.pulledTags = [];
    },
    //Called by the initializestore action to update the store's age range options.
    addAgeRanges(state, ageRanges){
      state.ageRangeOptions = ageRanges
    },
    changeValue(state, payload){
      let foundIndex =  state.pulledTags.findIndex(x => x.id == payload[0]);
      state.pulledTags[foundIndex].quantity = payload[1]
    },
    updatePulledTagFulfillment(state, payload){
      
      state.pulledTags.forEach(e => {
        if(!e.requireFinancialDonation) {
          e.fulfillment = payload;
        } else {
          e.fulfillment = 'donation';
        }
      })
    },
    //Are called by the changes in the app.vue selects and update the store values.
    updateSelectedGenders(state,selectedGenders){
      state.selectedGenders = selectedGenders;
    },
    updateSelectedAgeRanges(state,selectedAgeRanges){
      state.selectedAgeRanges = selectedAgeRanges;
    },
    updateSelectedCampus(state,selectedCampus){
      state.selectedCampus = selectedCampus;
    },
    addPulledtag(state, tag){
      let tagInfo = {
        id: tag.id,
        accountId: tag.accountId,
        quantity: tag.quantity,
        suggestedDonation: tag.suggestedDonation,
        requireFinancialDonation: tag.requireFinancialDonation,
        fulfillment: 'donation'
      }
      state.pulledTags.push(tagInfo);
    },
    setCurrentPerson(state, Person){
      state.currentPerson = {
        ...state.currentPerson,
        firstName: Person.FirstName,
        lastName: Person.LastName,
        email: Person.Email,
        id: Person.Id,

      }
    },


    updateFinancialData(state,financialData){

      state.financialData = {
        ...state.Financialdata,
        TransactionCode:financialData.TransactionCode,
        TransactionDateTime:financialData.TransactionDateTime,
        PrimaryPerson:financialData.PrimaryPerson,
        TotalAmount:financialData.TotalAmount,
        PaymentType: financialData.PaymentType,
        SuccessText:financialData.SuccessText,
        TransactionId:financialData.TransactionGuid
      }
    },
    updateTagsProcessed(state, value){
      state.tagsProcessed = value;
      
    },
    clearProcessedTags(state){
      let pulledIds = state.pulledTags.map(tag => tag.id);  
      state.tagList = state.pulledTags.filter(tag => pulledIds.includes(tag.id) == false);
      state.pulledTags = []
    },
    gettingTags(state,val){
      state.gettingTags = val
    }
  },
  actions: {
    //Processing tags when checking out the
    async processTags({commit, state}) {
      
      let financialDonationTags = state.pulledTags.filter(e => e.fulfillment == 'donation').map(e => e.id);
      let buyGiftsTags = state.pulledTags.filter(e => e.fulfillment == 'buygifts').map(e => e.id);
      let tagList ={
        financialTags: financialDonationTags.length > 0 ? financialDonationTags.join(',') : null,
        giftDonationTags: buyGiftsTags.length > 0  ? buyGiftsTags.join(',') : null,
      }
      
        let tagBody = {
          firstName: state.currentPerson.firstName,
          lastName: state.currentPerson.lastName,
          email: state.currentPerson.email,
          transactionId: state.financialData.TransactionId,
          tagList: tagList,
        }
        
        try {
        let response = await fetch(processTagsUrl,{
            // method:'POST',
            // headers:{
            //   'Content-Type':'application/json',
            // },
            // credentials:'include',
            // body: JSON.stringify(tagBody)
       })
       
       commit('clearProcessedTags')
       commit('updateTagsProcessed',true)
       
      }
      catch(err) {
        console.log(err)

      }
    },



    // action get tags from the back end  the tagListURl is set above so it can be changed from development to production. 
    // the new tags are filtered for any tags that have already been downloaded and only new tags are added.
    async getTags({commit, getters}){
        commit('gettingTags',true)
        //get list of current tags.
        let currentTags = getters.currentTagIds;
        //determin start which is the current tags length. this is set to the endpoint as and offset
        let start = currentTags.length
        //Determine the number of rows to fetch which is the stepsize.  
        let end = getters.getCurrentSteps.stepSize;
        
        if(process.env.NODE_ENV != 'development') {
        //fetch the next end# of rows ofsetting by start.  Initial request would be offset 0 number of rows 15.
        tagListUrl += `/${start}/${end}`        
        }
        
        let response = await fetch(tagListUrl);
        let tags  = await response.json(); 
      
        //Remove from response any tags currently downloaded
        if(currentTags.length > 0 ){
          tags = tags.filter( tag => currentTags.includes(tag.id) === false );
        } 

        //Send the new list of tags to append to the current tag list.
        commit('updateTags',tags)
        commit('gettingTags',false)
      },

    // Get Age ranges
    async getAgeRanges({commit}){
      let response = await fetch(ageRangesUrl);
      let ageRanges = await response.json();
      
      commit('addAgeRanges',ageRanges);

    },

    //Called when the app is Created and gets the first set of tags and pulled tags from local storage.
    initializeStore({commit, dispatch}){
        // get Age Range Options
        dispatch('getAgeRanges')
        
        //Gets thee current Person from the getCurrentPerson Action
        dispatch('getCurrentPerson')
    },


    async getCurrentPerson({commit, dispatch}){
      try {
        let response = await fetch(getCurrentPersonUrl,{
            credentials:'include'
          })
          
          let person = await response.json()
          commit('setCurrentPerson', person)
      }
     catch(err){
     // Do nothing as the current person is null and that does not error the application. 
     }
    },
  },
  
  plugins:[listenforIframeMessage],
  modules: {
  }
})
