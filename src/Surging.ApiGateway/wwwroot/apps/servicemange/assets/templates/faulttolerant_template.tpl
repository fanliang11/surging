{{each Entity}}
	<tr>
           <td class="center">
					{{if ($index+1)%2==1 }}
                    <span class="badge badge-grey ">${$index+1}</span>
                    {{else}}
                    <span class="badge   badge-success  ">${$index+1}</span>
                    {{/if}}
            </td>
            <td class="center"> ${ServiceId}</td>
            <td class="center"> 
					{{if CircuitBreakerForceOpen==true }}
					<span class="label label-success arrowed-in arrowed-in-right">是</span>
					{{else}}
					<span class="label label-danger arrowed">否</span>
					{{/if}}
			</td>
            <td class="center">
			${Strategy}
			</td>
            <td class="center">${ExecutionTimeoutInMilliseconds}</td>
            <td class="center">
					{{if RequestCacheEnabled==true }}
					<span class="label label-success arrowed-in arrowed-in-right">是</span>
					{{else}}
							<span class="label label-danger arrowed">否</span>
					{{/if}}
			</td>      
			<td class="center">${FallBackName}</td>  
			<td class="center">${Injection}</td>     
			<td class="center">${InjectionNamespaces}</td>   
			<td class="center">${BreakeErrorThresholdPercentage}</td>   
			<td class="center">${BreakeSleepWindowInMilliseconds}</td>   
			<td class="center">
					{{if BreakerForceClosed==true }}
					<span class="label label-success arrowed-in arrowed-in-right">是</span>
					{{else}}
							<span class="label label-danger arrowed">否</span>
					{{/if}}
			</td>   
			<td class="center">${BreakerRequestVolumeThreshold}</td>  
			<td class="center">${MaxConcurrentRequests}</td>  
			<td class="center"><div class="visible-md visible-lg hidden-sm hidden-xs action-buttons">
                    <a href="javascript:void(0)" class="grey editFaultTolerant"><i class="icon-lightbulb  bigger-120"></i>编辑</a>
                    </div>
			</td>
                           
    </tr>
{{/each}}
