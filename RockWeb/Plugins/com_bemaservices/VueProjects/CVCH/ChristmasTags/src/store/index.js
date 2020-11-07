import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

const taglistUrl = '/backend/tagsList.json';

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
    // From TagList.Vue
    taglist: [],
    start: 0,
    stepSize: 15,
    step:2,

  },
  getters:{
    
    
    
    filterTags(state){
      let filteredList = state.taglist;
      let pulledIds = state.pulledTags.map(tag => tag.id);  

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
      
    },
  },
  mutations: {
    updateTags(state, tags) {
      state.taglist = tags;
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
    }
  },
  actions: {
    // action get tags from the back end  the taglistURl is set above so it can be changed from development to production.
    async getTags({commit}){
      console.log('action started')
      let response = await fetch(taglistUrl);
      console.log(response)
      let tags  = await response.json(); 
      console.log(tags)
      commit('updateTags',tags)

    }
  },
  modules: {
  }
})
