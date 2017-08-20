       {{each entity}}
	<tr>
                            <td class="center">
								  {{if ($index+1)%2==1 }}
                                  <span class="badge badge-grey ">${$index+1}</span>
                                  {{else}}
                                  <span class="badge   badge-success  ">${$index+1}</span>
                                  {{/if}}
                            </td>
                      <td class="center"> ${id}</td>
                                        <td class="center"> ${metadatas.GroupName}</td>
                                        <td class="center">
										 {{if metadatas.WaitExecution==true }}
								是
								{{else}}
									否
								{{/if}}
										</td>
                                        <td class="center">${metadatas.Director}</td>
                                        <td class="center">${metadatas.Date}</td>         
										  <td class="center"><div class="visible-md visible-lg hidden-sm hidden-xs action-buttons">
                                                   <a href="/ServiceManage/FaultTolerant?serviceId={{= id}}" nodemenu="" class="grey editRegion" data-pjax=".page-content"><i class="icon-lightbulb  bigger-120"></i>查看容错规则</a>

                             </div></td>
                           
                        </tr>
						  {{/each}}