{{each entity}}
	<tr>
            <td class="center">
					{{if ($index+1)%2==1 }}
                    <span class="badge badge-grey ">${$index+1}</span>
                    {{else}}
                    <span class="badge   badge-success  ">${$index+1}</span>
                    {{/if}}
            </td>
            <td class="center"> ${serviceId}</td>
            <td class="center"> 
					{{if circuitBreakerForceOpen==true }}
					<span class="label label-success arrowed-in arrowed-in-right">是</span>
					{{else}}
					<span class="label label-danger arrowed">否</span>
					{{/if}}
			</td>
            <td class="center">
			${strategy}
			</td>
            <td class="center">${executionTimeoutInMilliseconds}</td>
            <td class="center">
					{{if requestCacheEnabled==true }}
					<span class="label label-success arrowed-in arrowed-in-right">是</span>
					{{else}}
							<span class="label label-danger arrowed">否</span>
					{{/if}}
			</td>      
			<td class="center">${injection}</td>     
			<td class="center">${injectionNamespaces}</td>   
			<td class="center">${breakeErrorThresholdPercentage}</td>   
			<td class="center">${breakeSleepWindowInMilliseconds}</td>   
			<td class="center">
					{{if breakerForceClosed==true }}
					<span class="label label-success arrowed-in arrowed-in-right">是</span>
					{{else}}
							<span class="label label-danger arrowed">否</span>
					{{/if}}
			</td>   
			<td class="center">${breakerRequestVolumeThreshold}</td>  
			<td class="center">${maxConcurrentRequests}</td>  
			<td class="center"><div class="visible-md visible-lg hidden-sm hidden-xs action-buttons">
                    <a href="javascript:void(0)" class="grey editFaultTolerant"><i class="icon-lightbulb  bigger-120"></i>编辑</a>
                    </div>
			</td>
                           
    </tr>
{{/each}}