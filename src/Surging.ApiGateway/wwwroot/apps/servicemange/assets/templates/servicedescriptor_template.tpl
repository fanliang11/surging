       {{each Entity}}
	<tr>
                            <td class="center">
								  {{if ($index+1)%2==1 }}
                                  <span class="badge badge-grey ">${$index+1}</span>
                                  {{else}}
                                  <span class="badge   badge-success  ">${$index+1}</span>
                                  {{/if}}
                            </td>
                      <td class="center"> ${Id}</td>
                                        <td class="center"> ${Metadatas.GroupName}</td>
                                        <td class="center">
										 {{if Metadatas.WaitExecution==true }}
								是
								{{else}}
									否
								{{/if}}
										</td>
									<td class="center">
										 {{if Metadatas.DisableNetwork==true }}
								是
								{{else}}
									否
								{{/if}}
										</td>
												<td class="center">
										 {{if Metadatas.EnableAuthorization==true }}
								是
								{{else}}
									否
								{{/if}}
										</td>
                                        <td class="center">${Metadatas.Director}</td>
                                        <td class="center">${Metadatas.Date}</td>         
										  <td class="center"><div class="visible-md visible-lg hidden-sm hidden-xs action-buttons">
                                                   <a href="/ServiceManage/FaultTolerant?serviceId={{= Id}}" nodemenu="" class="grey editRegion" data-pjax=".page-content"><i class="icon-lightbulb  bigger-120"></i>查看容错规则</a>
												    <a href="/ServiceManage/ServiceSubscriber?serviceId={{= Id}}" nodemenu="" class="grey editRegion" data-pjax=".page-content"><i class="icon-lightbulb  bigger-120"></i>查看订阅者</a>

                             </div></td>
                           
                        </tr>
						  {{/each}}