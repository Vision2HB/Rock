import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
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
  },
  mutations: {
  },
  actions: {
  },
  modules: {
  }
})
