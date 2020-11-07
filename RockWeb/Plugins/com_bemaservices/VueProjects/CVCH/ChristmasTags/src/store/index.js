import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    // From App.Vue
    pulledTags:[],
    selectedGenders:[],
    selectedAgeRanges:[],
    selectedCampus: personCampus,
    pulledTags:[],
    campusOptions:campuses,
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
    
    
    
    filterTags(){
      let filteredList = this.$state.taglist;
      let pulledIds = this.$state.pulledTags.map(tag => tag.id);  

      if(pulledIds && pulledIds.length > 0 ) {
        filteredList = filteredList.filter(tag => pulledIds.includes(tag.id) == false)
      }

      if(this.$state.selectedGenders && this.$state.selectedGenders.length > 0){
        filteredList = filteredList.filter(tag => this.$state.selectedGenders.includes(tag.gender.id) == true)
      }

      if(this.$state.selectedAgeRanges && this.$state.selectedAgeRanges.length > 0){
        filteredList = filteredList.filter(tag => this.$state.selectedAgeRanges.includes(tag.ageRange.id) == true)
      }
      if(this.$state.selectedCampus && this.$state.selectedCampus> 0){
        filteredList = filteredList.filter(tag => this.$state.selectedCampus == tag.campusId)
      }
      return filteredList.slice(0,this.$state.step * this.$state.stepSize);
      
    },
  },
  mutations: {
  },
  actions: {
  },
  modules: {
  }
})
