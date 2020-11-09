import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

let tagListUrl = '/Webhooks/Lava.ashx/BEMA/GetChristmasTags';
let ageRangesUrl = '/Webhooks/Lava.ashx/BEMA/GetAgeRanges';
let processTagsUrl = '/Webhooks/Lava.ashx/BEMA/ProcessChristmasTags';
let getCurrentPersonUrl = '/api/People/GetCurrentPerson';

if(process.env.NODE_ENV == 'development') {
   tagListUrl = '/backend/tagsList.json';
   ageRangesUrl = '/backend/ageRanges.json';
   getCurrentPersonUrl = '/backend/currentPerson.json';
}

export default new Vuex.Store({
  state: {
    // From App.Vue
    selectedGenders:[],
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
    stepSize: 15,
    step:2,
    currentPerson:{
      firstName:null,
      lastName:null,
      email:null,
      personAliasId:null
    }
  },
  getters:{
    currentTagIds(state) {
      return state.tagList.map(tag => tag.id)
    },
    getAccountTotals(state){

      var result = [];
      state.pulledTags.reduce(function (res, value) {
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
  },
  mutations: {
    // Initiated by the get tags action to concat the results into the tagsList store item.
    updateTags(state, tags) {
        state.tagList = state.tagList.concat(tags);
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
      state.pulledTags.push(tag);
    },
    setCurrentPersonFirstName(state, firstName){
      state.currentPerson.firstName = firstName;
    },
    setCurrentLastName(state, lastName){
      state.currentPerson.lastName = lastName
    },
    setCurrentEmail(state, email){
      state.currentPerson.email = email
    },
    setCurrentPersonAliasId(state,id){
      state.currentPerson.currentPersonAliasId = id
    }
  },
  actions: {
    //Processing tags when checking out the
    async processTags({commit, state}) {
      let pulledTagIds = this.$store.state.pulledTagIds;
            let url = processTagsUrl + `/${this.form.firstName}/${this.form.lastName}/${this.form.email}/${pulledTagIds.join(',')}/${this.transactionInfo ? this.transactionInfo.PrimaryPerson : 0}/${this.transactionInfo ? this.transactionInfo.TransactionGuid: 0}`
            let response = await fetch(url)
            let data = await response.json()
      
            this.responseMessage = data.SuccessText;
            this.tagResponse = data;
            this.showSuccess = true;
    },



    // action get tags from the back end  the tagListURl is set above so it can be changed from development to production. 
    // the new tags are filtered for any tags that have already been downloaded and only new tags are added.
    async getTags({commit, getters}){
        let response = await fetch(tagListUrl);
        let tags  = await response.json(); 
        const currentTags = getters.currentTagIds;
        
        if(currentTags.length > 0 ){
          tags = tags.filter( tag => currentTags.includes(tag.id) === false );
        } 
        commit('updateTags',tags)
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

        // Gets new tags from endpont using the getTags store action
        dispatch('getTags')
        
        //Gets thee current Person from the getCurrentPerson Action
        dispatch('getCurrentPerson')
    },

    async getCurrentPerson({commit, dispatch}){
      let response = await fetch(getCurrentPersonUrl,{
          credentials:'include'
        })

      let person = await response.json()
        commit('setCurrentPersonFirstName', person.FirstName)
        commit('setCurrentLastName', person.LastName)
        commit('setCurrentEmail', person.Email)
        commit('setCurrentPersonAliasId',person.PrimaryAliasId)
      }
  },

  modules: {
  }
})
