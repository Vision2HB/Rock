<template>
  <v-app>
    <v-img
        contain
        max-height="250"
        src="./Assets/images/CWTC_Title_English.jpg"
      ></v-img>

     <v-sheet height="64"
     
     class="pt-8 mb-8 "
     
     >
        <v-toolbar flat :bottom="true">
          <v-select
            background-color="transparent"
              style="width:25%"
              class="personList"
              cols="4"
              :menu-props="{ top: false, offsetY: true }"
              v-model="selectedGenders"
              :items="genderOptions"
              item-text="description"
              item-value="id"
              :deletable-chips="true"
              label="Filter By Gender"
              multiple
              chips
            >
            </v-select>
       
        <v-spacer></v-spacer>
       
          <v-select
              style="width:25%"
              class="personList"
              cols="4"
              :menu-props="{ top: false, offsetY: true }"
              v-model="selectedAgeRanges"
              :items="ageRangeOptions"
              item-text="description"
              item-value="id"
              :deletable-chips="true"
              label="Filter By Age Ranges"
              multiple
              chips
            >
            </v-select>

        <v-spacer>
        </v-spacer>
        <v-select
            background-color="transparent"
              style="width:25%"
              class="personList"
              cols="4"
              :menu-props="{ top: false, offsetY: true }"
              v-model="selectedCampus"
              :items="campusOptions"
              item-text="description"
              item-value="id"
              :deletable-chips="true"
              label="Filter By Campus"

            >
            </v-select>
        </v-toolbar>
     </v-sheet>

    <v-main class="snowflake">

              <v-btn
                id="tagButton"
                @mouseover="showCount=false"
                @mouseleave="showCount=true"
                @click="showForm"
                class="accent"
                fixed
                x-large
                bottom
                left
                fab
                style="z-index:2000"
              >
            
               <span id="pulledCount" class="fa-stack" :class="{hide:!showCount}">
                   <!-- Create an icon wrapped by the fa-stack class -->
                
                    <!-- The icon that will wrap the number -->
                    <span class="fas fa-tags fa-stack-2x"></span>
                    <!-- a strong element with the custom content, in this case a number -->
                    <transition name="slide-fade" mode="out-in">
                        <strong :key="pulledTags.length" class="fa-stack-1x accent--text" style="font-weight: 900; padding-left:5px; transform:translate(-3px,1px) scale(1.2); font-size:13px;">
                            {{pulledTags.length}}    
                        </strong>
                    </transition>        
                
                </span>
            
              
               <span :class="{hide:showCount}"><i class="far fa-list-alt fa-2x"></i></span>
              
              </v-btn>
              
            
       <TagList />
      
      <ContactForm
        class="vuemodal"
        :class="{showItem:this.showModal}"
        :hideForm="showModal"
        :tags="this.pulledTags"
        v-on:close-modal="this.showForm"
         />

    </v-main>
  </v-app>
</template>

<script>
import TagList from "./components/taglist.vue";

  const ContactForm = () => import(
    /* webpackChunkName: "ContactForm" */ './components/contactform.vue'
  );

import { gsap } from "gsap";

export default {
  name: 'App',

  components: {
    TagList,
    ContactForm
  },

  data: () => ({
    hidden:true,
    showCount:true,
    showModal:false,

    //
  }),
   
  watch:{
     
    //  pulledTags: function(){
    //      localStorage.setItem('pulledTags',JSON.stringify(this.pulledTags.map(tag => tag.id)));
    //  },

  },
  computed:{
    //Pulls information from the store for the various filters and display info.
    //Used to show the number of tags pulled in the icon button(.length) and passed to the form for display.
    pulledTags(){
        return this.$store.state.pulledTags
    },

    // The following three items are used for the filters.
    ageRangeOptions(){
      return this.$store.state.ageRangeOptions
    },
    campusOptions(){
      return this.$store.state.campusOptions
    },

    genderOptions(){
      return this.$store.state.genderOptions
    },
    

    // When binding to a value in a vuex store you need to use a two way computed property that has a get() function to get a value, and a set(value) that calls a mutation to set a value.  This preserves the two way binding.
    selectedGenders: {
      get(){
        return this.$store.state.selectedGenders;
      },
      set(value){
        this.$store.commit('updateSelectedGenders',value);
      }
    },

    selectedAgeRanges: {
      get(){
        return this.$store.state.selectedGenders;
      },
      set(value){
        this.$store.commit('updateSelectedAgeRanges',value);
      }
    },


      selectedCampus: {
        get() {
          return this.$store.state.selectedCampus
        },
        set(value) {
          this.$store.commit('updateSelectedCampus',value)
        }
      }


  },
  methods: {
    pullTag(tag){
        this.$store.commit('addPulledtag',tag)
    },
    showForm() {
        console.log('showform')
        this.showModal = !this.showModal;
        if(this.showModal === true ) {
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = 'inherit';
        }
    },

  },
   created () {

     this.$store.dispatch('initializeStore')
    
  },
 
};
</script>


<style>
@import url('https://fonts.googleapis.com/css2?family=Lato:wght@400;700;900&display=swap');

.v-application {
   /* background-image: url("./assets/images/CWTC_BlueSnowBackground.jpg"); */
  background-repeat: repeat;
  border: 2rem solid var(--v-primary-base,green);
  padding-bottom:20px;
  font-family: 'Lato', sans-serif !important;
}
</style>
<style scoped>
*, *:after, *:before {
  box-sizing: border-box;
}


.btn {
    font-size:1.2rem;
}



/* Enter and leave animations can use different */
/* durations and timing functions.              */
.slide-fade-enter-active {
  transition: all .3s ease;
}
.slide-fade-leave-active {
  transition: all .8s cubic-bezier(1.0, 0.5, 0.8, 1.0);
}
.slide-fade-enter, .slide-fade-leave-to
/* .slide-fade-leave-active below version 2.1.8 */ {
  transform: translateX(10px);
  opacity: 0;
}
.vuemodal {
    position:fixed;
    bottom:0;
    left:0;
    height:100vh;
    width: 100vw;
    transform: scale(0);
    
    z-index:30000;
    transform-origin:bottom left;
    display:flex;
    flex-direction:column;
    justify-content:space-around;
    align-items:center;
    transition: all 500ms ease;
}
.vuemodal.showItem{
    transform:scale(1);
    transition: all 500ms ease;
}


</style>
