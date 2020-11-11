<template>
  <tr>
      <td>{{tagInfo.ageRange.description}}</td>
      <td>{{tagInfo.gender.description}}</td>
      <td>{{tagInfo.description}} <span v-if="tagInfo.requireFinancialDonation" class="warning--text"><br />This tag is fulfilled by a financial donation.</span></td>
      <td class="text-center" style="width:100px;">
        
          <v-select
          v-if="tagInfo.allowMultiple"
          :items="getNumbers(tagInfo.quantityRemaining)"
          class="text-center"
          @change="changeValue(pulledTag.id, pulledTag.quantity)"
          v-model="pulledTag.quantity"

          >
          <template v-slot:selection="{ item }">
            <span style="transform: translateX(-50%);
    position: absolute;
    left: 50%;
    width: 2ch;">
              {{ item }}
            </span>
          </template>
          
          </v-select>
          <span v-else>{{pulledTag.quantity}}</span>
      </td>
      
      <td v-if="fulfillment === 'donation' || tagInfo.requireFinancialDonation || pulledTag.fulfillment == 'donation'">${{(pulledTag.quantity * pulledTag.suggestedDonation).toFixed(2)}}</td>
      <td v-else>Buy Gifts</td>
      <td class="text-center">
          <v-icon @click="$emit('remove-tag',tagInfo.id)" color="secondary" small>
              fa-trash</v-icon>
      </td>

  </tr>
</template>

<script>
export default {
  props: {
    pulledTag: {
      type: Object,
      required: true
    },
    value: {
      type: Number,
      required: false,
      default:1
    },
    fulfillment: {
      type: String,
    },
  },
  
  computed:{
    getTotal(){
      return state.getters.getAccountTotals.reduce((acc, val) => {
        return accumulator + (val.suggestedDonation * val.quantity)
      },0)
    },
    tagInfo(){
      return this.$store.getters.getTag(this.pulledTag.id)
    }
  },
  
  methods: {
    getNumbers(maxValue){
      return Array(maxValue).fill().map((_, idx) => 1 + idx)
    
    },
    removeTag(id){
      console.log(id)
    },
    changeValue(id,value){
      this.$store.commit('changeValue',[id,value])
    }

  }

}
</script>

<style>

</style>