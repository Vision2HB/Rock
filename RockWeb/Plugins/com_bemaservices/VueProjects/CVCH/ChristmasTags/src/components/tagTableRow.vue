<template>
  <tr>
      <td>{{tag.ageRange.description}}</td>
      <td>{{tag.gender.description}}</td>
      <td>{{tag.description}} <span v-if="tag.requireFinancialDonation" class="warning--text"><br />This tag is fulfilled by a financial donation.</span></td>
      <td class="text-center" style="width:100px;">
        
          <v-select
          v-if="tag.allowMultiple"
          :items="getNumbers(tag.quantityRemaining)"
          class="text-center"
          @change="changeValue(tag.quantity)"
          v-model="tag.quantity"

          ></v-select>
          <span v-else>1</span>
      </td>
      
      
      <td class="text-center">
          <v-icon @click="$emit('remove-tag',tag.id)" color="secondary" small>
              fa-trash</v-icon>
      </td>

  </tr>
</template>

<script>
export default {
  props: {
    tag: {
      type: Object,
      required: true
    },
    value: {
      type: Number,
      required: false,
      default:1
    }
  },
  
  
  methods: {
    getNumbers(maxValue){
      return Array(maxValue).fill().map((_, idx) => 1 + idx)
    
    },
    removeTag(id){
      console.log(id)
    },
    changeValue(value){
      console.log(value)
    }

  }

}
</script>

<style>

</style>