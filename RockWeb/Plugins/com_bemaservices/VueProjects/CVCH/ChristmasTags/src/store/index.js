import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

const tagListUrl = '/backend/tagsList.json';
const ageRangesUrl = '/backend/ageRanges.json';

export default new Vuex.Store({
  state: {
    // From App.Vue
    pulledTags:[],
    selectedGenders:[],
    selectedAgeRanges:[],
    selectedCampus: 1,
    pulledTags:[],
    campusOptions:[{id:1, name:'Mooresville Campus'}],
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

  },
  getters:{
    currentTagIds(state) {
      return state.tagList.map(tag => tag.id)
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
    //Called by the initializestore action to update the store's age range options.
    addAgeRanges(state, ageRanges){
      console.log(ageRanges)
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
      state.pulledTags.push(tag.id);
    },


  },
  actions: {
    // action get tags from the back end  the tagListURl is set above so it can be changed from development to production. 
    // the new tags are filtered for any tags that have already been downloaded and only new tags are added.
    async getTags({commit, getters}){
        let response = await fetch(tagListUrl);
        let tags  = await response.json(); 
        const currentTags = getters.currentTagIds;
        
        console.log(tags.length)
        if(currentTags.length > 0 ){
          tags = tags.filter( tag => currentTags.includes(tag.id) === false );
        } 
        console.log(tags)
        commit('updateTags',tags)

      },

    // Get Age ranges
    async getAgeRanges({commit}){
      let response = await fetch(ageRangesUrl);
      let ageRanges = await response.json()
      commit('addAgeRanges',ageRanges)

    },

    //Called when the app is Created and gets the first set of tags and pulled tags from local storage.
    initializeStore({commit, dispatch,}){
        // get Age Range Options
        dispatch('getAgeRanges')

        // Gets new tags from endpont using the getTags store action
        dispatch('getTags')
        
        // Get Tags From Local StorageId and add each to the vuex store pulled Tags
        const tagList = JSON.parse(localStorage.getItem('pulledTags'))
        tagList.forEach(tag => commit('addPulledtag',{id: tag}))
    }
  },
  modules: {
  }
})
