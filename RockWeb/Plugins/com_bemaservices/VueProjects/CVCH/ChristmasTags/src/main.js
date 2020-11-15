import Vue from 'vue'
import App from './App.vue'
import vuetify from './plugins/vuetify';
import store from './store'


// You can put any js variables you want to set from lava here. The current format has an api call to get the current person and fill the tags. however, I could have used the variables here an preloaded everything.  Be sure to update the vuex store or the component code to use these variables instead of the api call.

  // let firstName = '{{CurrentPerson.NickName}}';
  // let lastName = 'CurrentPerson.LastName';
  // let email = '{{CurrentPerson.Email}}';


Vue.config.productionTip = true

new Vue({
  vuetify,
  store,
  render: h => h(App)
}).$mount('#app')
